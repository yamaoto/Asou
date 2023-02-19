using Asou.Abstractions;
using Asou.Abstractions.Events;
using Asou.Abstractions.Process.Contract;
using Asou.Abstractions.Process.Execution;
using Asou.Abstractions.Process.Instance;
using Asou.Core.Commands;
using Asou.Core.Commands.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Asou.Core;

public class ProcessExecutionEngine
{
    private readonly IProcessExecutionDriver _driver;
    private readonly IEnumerable<IInitializeHook> _initializers;

    // TODO: Handle instance deletion after execution done in _instances
    private readonly Dictionary<Guid, IProcessInstance> _instances = new();
    private readonly ILogger<ProcessExecutionEngine> _logger;
    private readonly ScopedCqrsRunner _scopedCqrsRunner;

    public ProcessExecutionEngine(
        IEnumerable<IInitializeHook> initializers,
        IProcessExecutionDriver driver,
        ILogger<ProcessExecutionEngine> logger,
        ScopedCqrsRunner scopedCqrsRunner)
    {
        _initializers = initializers;
        _driver = driver;
        _logger = logger;
        _scopedCqrsRunner = scopedCqrsRunner;
    }

    public async Task InitializeAsync()
    {
        foreach (var initializer in _initializers)
        {
            // TODO: Handle initialize with conditions to control enabled / disabled
            await initializer.Initialize();
        }

        // Resume execution after restart
        await ResumeRunningProcessesAsync();
    }

    public async Task StopExecutionAsync(CancellationToken cancellationToken)
    {
        var storeProcessParametersArray = _instances
            .Where(w => w.Value.PersistenceType != PersistenceType.No)
            .Select(s => new StoreProcessParameters(s.Key, s.Value.ProcessRuntime.Parameters.ToValueContainerMap()))
            .ToArray();
        using var storeProcessParametersCommand = _scopedCqrsRunner.Get<StoreProcessParametersCommand>();
        await storeProcessParametersCommand.Handler.ActivateAsync(storeProcessParametersArray, cancellationToken);
    }

    public async Task<ProcessParameters?> CreateAndExecuteAsync(Guid processContractId, ProcessParameters parameters,
        ExecutionOptions? executionOptions = null, CancellationToken cancellationToken = default)
    {
        using var getProcessContractRequest = _scopedCqrsRunner.Get<GetProcessContractRequest>();
        var processContract =
            await getProcessContractRequest.Handler.RequestAsync(processContractId, cancellationToken);

        if (processContract == null)
        {
            throw new Exception("processContract not exists");
        }

        return await ExecuteAsync(processContract, Guid.NewGuid(), parameters,
            executionOptions ?? new ExecutionOptions(), cancellationToken);
    }

    public async Task<ProcessParameters?> CreateAndExecuteAsync(Guid processContractId, Guid processVersionId,
        int versionNumber,
        ProcessParameters parameters,
        ExecutionOptions? executionOptions = null,
        CancellationToken cancellationToken = default)
    {
        using var getProcessContractRequest = _scopedCqrsRunner.Get<GetProcessContractRequest>();
        var processContract =
            await getProcessContractRequest.Handler.RequestAsync(processContractId, cancellationToken);

        if (processContract == null)
        {
            throw new Exception("processContract not exists");
        }

        return await ExecuteAsync(processContract, Guid.NewGuid(), parameters,
            executionOptions ?? new ExecutionOptions(), cancellationToken);
    }

    private async Task<ProcessParameters?> ExecuteAsync(ProcessContract processContract, Guid processInstanceId,
        ProcessParameters parameters, ExecutionOptions executionOptions, CancellationToken cancellationToken = default)
    {
        var instance =
            await _driver.CreateInstanceAsync(processContract, processInstanceId, parameters, cancellationToken);
        _instances[instance.Id] = instance;
        var state = ProcessInstanceState.New;
        using var insertProcessInstanceStateCommand = _scopedCqrsRunner.Get<InsertProcessInstanceStateCommand>();
        await insertProcessInstanceStateCommand.Handler.ActivateAsync(instance, state,
            cancellationToken);
        using var updateProcessInstanceStateCommand = _scopedCqrsRunner.Get<UpdateProcessInstanceStateCommand>();
        try
        {
            state = ProcessInstanceState.Running;
            await updateProcessInstanceStateCommand.Handler.ActivateAsync(instance.Id, state, cancellationToken);

            var result = await _driver.RunAsync(instance, executionOptions, cancellationToken);
            state = ProcessInstanceState.Finished;
            return result;
        }
        catch (Exception e)
        {
            state = ProcessInstanceState.Error;
            // TODO: Provide error information to process instance
            _logger.LogError(e, "Process {ProcessContractName} ID {ProcessInstanceId} thrown error",
                instance.ProcessContract.Name, instance.Id);
            throw;
        }
        finally
        {
            await updateProcessInstanceStateCommand.Handler.ActivateAsync(instance.Id, state, cancellationToken);
        }
    }

    private async Task ResumeExecutionAsync(ProcessContract processContract, Guid processInstanceId,
        ProcessParameters parameters, CancellationToken cancellationToken = default)
    {
        var instance =
            await _driver.CreateInstanceAsync(processContract, processInstanceId, parameters, cancellationToken);
        _instances[instance.Id] = instance;
        using var updateProcessInstanceStateCommand = _scopedCqrsRunner.Get<UpdateProcessInstanceStateCommand>();
        // Override any other state
        await updateProcessInstanceStateCommand.Handler.ActivateAsync(instance.Id, ProcessInstanceState.Running,
            cancellationToken);

        // Resume execution
        await _driver.ResumeAsync(instance, cancellationToken);
    }

    internal async Task HandleEventAsync(EventSubscriptionModel subscription, EventRepresentation eventRepresentation,
        CancellationToken cancellationToken = default)
    {
        // Check if instance is active
        if (!_instances.TryGetValue(subscription.ProcessInstanceId, out var instance))
        {
            // TODO: Resume instance if not present
            throw new NotImplementedException();
        }

        await instance.HandleSubscriptionEventAsync(subscription, eventRepresentation, cancellationToken);
    }

    private async Task ResumeRunningProcessesAsync(CancellationToken cancellationToken = default)
    {
        using var getProcessesForResumeRequest = _scopedCqrsRunner.Get<GetProcessesForResumeRequest>();
        var resumeProcessRecords = getProcessesForResumeRequest.Handler.GetAsyncEnumerable(cancellationToken);
        await foreach (var (instance, processContract, parameters) in resumeProcessRecords.WithCancellation(
                           cancellationToken))
        {
            if (processContract == null)
            {
                // TODO: Log warn event
                continue;
            }

            await ResumeExecutionAsync(processContract, instance.Id, new ProcessParameters(parameters),
                cancellationToken);
        }
    }
}
