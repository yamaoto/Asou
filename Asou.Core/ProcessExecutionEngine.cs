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

    private readonly SemaphoreSlim _instanceCreationSemaphore = new(1, 1);

    // TODO: Handle instance deletion after execution done in _instances
    private readonly Dictionary<Guid, IProcessInstance> _instances = new();
    private readonly ILogger<ProcessExecutionEngine> _logger;
    private readonly ScopedActionRunner _scopedActionRunner;

    public ProcessExecutionEngine(
        IEnumerable<IInitializeHook> initializers,
        IProcessExecutionDriver driver,
        ILogger<ProcessExecutionEngine> logger,
        ScopedActionRunner scopedActionRunner)
    {
        _initializers = initializers;
        _driver = driver;
        _logger = logger;
        _scopedActionRunner = scopedActionRunner;
    }

    /// <summary>
    ///     Initialize process execution engine in application lifecycle. This method should be called at application startup.
    /// </summary>
    public async Task InitializeAsync()
    {
        foreach (var initializer in _initializers)
        {
            // TODO: Handle initialize with conditions to control enabled / disabled
            await initializer.Initialize();
        }

        // Resume execution for all running processes that are not finished
        await ScheduleResumeRunningProcessesAsync();
    }

    public async Task StopExecutionAsync(CancellationToken cancellationToken)
    {
        var storeProcessParametersArray = _instances
            .Where(w => w.Value.PersistenceType != PersistenceType.No)
            .Select(s => new StoreProcessParameters(s.Key, s.Value.ProcessRuntime.Parameters.ToValueContainerMap()))
            .ToArray();
        using var storeProcessParametersCommand = _scopedActionRunner.Get<StoreProcessParametersCommand>();
        await storeProcessParametersCommand.Handler.ActivateAsync(storeProcessParametersArray, cancellationToken);
    }

    /// <summary>
    ///     Create and execute new process instance with the latest active version
    /// </summary>
    /// <param name="processContractId">Process contract</param>
    /// <param name="parameters">Running process parameters</param>
    /// <param name="executionOptions">Execution parameters</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    ///     There tow scenarios:
    ///     1. When <see cref="ExecutionOptions.ExecutionFlowType" /> is <see cref="ExecutionFlowType.Synchronous" /> and
    ///     <see cref="ExecutionOptions.RunInBackground" /> is set to false, method will return
    ///     <see cref="ProcessParameters" />
    ///     when process instance is finished
    ///     2. Otherwise, method will return null and process instance will be executed in background thread.
    /// </returns>
    public async Task<ProcessParameters?> CreateAndExecuteAsync(Guid processContractId,
        ProcessParameters? parameters = null,
        ExecutionOptions? executionOptions = null, CancellationToken cancellationToken = default)
    {
        using var getProcessContractRequest = _scopedActionRunner.Get<GetProcessContractRequest>();
        var processContract =
            await getProcessContractRequest.Handler.RequestAsync(processContractId, cancellationToken);

        if (processContract == null)
        {
            throw new Exception("processContract not exists");
        }

        return await ExecuteAsync(processContract, Guid.NewGuid(), parameters ?? new ProcessParameters(),
            executionOptions ?? new ExecutionOptions(), cancellationToken);
    }

    /// <summary>
    ///     Create and execute new process instance with specific version
    /// </summary>
    /// <param name="processContractId">Process contract</param>
    /// <param name="processVersionId">Process version</param>
    /// <param name="versionNumber"></param>
    /// <param name="parameters">Running process parameters</param>
    /// <param name="executionOptions">Execution parameters</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    ///     There tow scenarios:
    ///     1. When <see cref="ExecutionOptions.ExecutionFlowType" /> is <see cref="ExecutionFlowType.Synchronous" /> and
    ///     <see cref="ExecutionOptions.RunInBackground" /> is set to false, method will return
    ///     <see cref="ProcessParameters" />
    ///     when process instance is finished
    ///     2. Otherwise, method will return null and process instance will be executed in background thread.
    /// </returns>
    public async Task<ProcessParameters?> CreateAndExecuteAsync(Guid processContractId, int versionNumber,
        ProcessParameters? parameters, ExecutionOptions? executionOptions = null,
        CancellationToken cancellationToken = default)
    {
        using var getProcessContractRequest = _scopedActionRunner.Get<GetProcessContractRequest>();
        var processContract =
            await getProcessContractRequest.Handler.RequestAsync(processContractId, cancellationToken);

        if (processContract == null)
        {
            throw new Exception("processContract not exists");
        }

        return await ExecuteAsync(processContract, Guid.NewGuid(), parameters ?? new ProcessParameters(),
            executionOptions ?? new ExecutionOptions(), cancellationToken);
    }

    private async Task<ProcessParameters?> ExecuteAsync(ProcessContract processContract, Guid processInstanceId,
        ProcessParameters parameters, ExecutionOptions executionOptions, CancellationToken cancellationToken = default)
    {
        var instance =
            await _driver.CreateInstanceAsync(processContract, processInstanceId, parameters, executionOptions,
                cancellationToken);
        _instances[instance.Id] = instance;
        var state = ProcessInstanceState.New;
        using var insertProcessInstanceStateCommand = _scopedActionRunner.Get<InsertProcessInstanceStateCommand>();
        await insertProcessInstanceStateCommand.Handler.ActivateAsync(instance, state,
            cancellationToken);
        using var updateProcessInstanceStateCommand = _scopedActionRunner.Get<UpdateProcessInstanceStateCommand>();
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

    /// <summary>
    ///     Resume execution for specific process instance.
    ///     This method should be called when application at startup scheduled to resume execution for all running processes
    /// </summary>
    /// <param name="processInstanceId"></param>
    /// <param name="cancellationToken"></param>
    internal async Task ResumeExecutionAsync(Guid processInstanceId, CancellationToken cancellationToken = default)
    {
        using var getProcessesForResumeRequest = _scopedActionRunner.Get<GetProcessesForResumeRequest>();
        var resumeRecord =
            await getProcessesForResumeRequest.Handler.GetProcessToResumeAsync(processInstanceId, cancellationToken);
        if (resumeRecord?.ProcessContract == null)
        {
            // TODO: Log warn event
            return;
        }

        await ResumeExecutionAsync(resumeRecord.ProcessContract, processInstanceId,
            new ProcessParameters(resumeRecord.Parameters),
            cancellationToken);
    }

    private async Task ResumeExecutionAsync(ProcessContract processContract, Guid processInstanceId,
        ProcessParameters parameters, CancellationToken cancellationToken = default)
    {
        var instance =
            await _driver.CreateInstanceAsync(processContract, processInstanceId, parameters, new ExecutionOptions(),
                cancellationToken);
        _instances[instance.Id] = instance;
        using var updateProcessInstanceStateCommand = _scopedActionRunner.Get<UpdateProcessInstanceStateCommand>();

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
        if (_instances.TryGetValue(subscription.ProcessInstanceId, out var instance))
        {
            await instance.HandleSubscriptionEventAsync(subscription, eventRepresentation, cancellationToken);
            return;
        }

        // lock to prevent duplicate instance creation
        await _instanceCreationSemaphore.WaitAsync(cancellationToken);
        try
        {
            // Check again in case another thread created the instance while waiting
            if (_instances.TryGetValue(subscription.ProcessInstanceId, out instance))
            {
                await instance.HandleSubscriptionEventAsync(subscription, eventRepresentation, cancellationToken);
                return;
            }

            using var getProcessesForResumeRequest = _scopedActionRunner.Get<GetProcessesForResumeRequest>();
            var resumeRecord =
                await getProcessesForResumeRequest.Handler.GetProcessToResumeAsync(subscription.ProcessInstanceId,
                    cancellationToken);
            if (resumeRecord?.ProcessContract == null)
            {
                // TODO: Log warn event
                return;
            }

            instance =
                await _driver.CreateInstanceAsync(resumeRecord.ProcessContract, resumeRecord.ProcessInstance.Id,
                    new ProcessParameters(resumeRecord.Parameters), new ExecutionOptions(),
                    cancellationToken);
            _instances[instance.Id] = instance;
        }
        finally
        {
            _instanceCreationSemaphore.Release();
        }

        await instance.HandleSubscriptionEventAsync(subscription, eventRepresentation, cancellationToken);
    }

    /// <summary>
    ///     At application startup, resume all running processes that are not finished by scheduling run with message queue
    ///     to prevent duplicate running process instances in horizontal scaling environment
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async Task ScheduleResumeRunningProcessesAsync(CancellationToken cancellationToken = default)
    {
        using var resumeProcessOnStartup = _scopedActionRunner.Get<ResumeProcessesOnStartup>();
        await resumeProcessOnStartup.Handler.ActivateAsync(cancellationToken);
    }
}
