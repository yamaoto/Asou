using Asou.Abstractions;
using Asou.Abstractions.Events;
using Asou.Abstractions.Process.Contract;
using Asou.Abstractions.Process.Execution;
using Asou.Abstractions.Process.Instance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Asou.Core;

public class ProcessExecutionEngine
{
    private readonly IProcessExecutionDriver _driver;
    private readonly IEnumerable<IInitializeHook> _initializers;

    // TODO: Handle instance deletion after execution done in _instances
    private readonly Dictionary<Guid, IProcessInstance> _instances = new();
    private readonly ILogger<ProcessExecutionEngine> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ProcessExecutionEngine(
        IServiceProvider serviceProvider,
        IEnumerable<IInitializeHook> initializers,
        IProcessExecutionDriver driver,
        ILogger<ProcessExecutionEngine> logger
    )
    {
        _serviceProvider = serviceProvider;
        _initializers = initializers;
        _driver = driver;
        _logger = logger;
    }

    public async Task HandleEventAsync(EventSubscriptionModel subscription, EventRepresentation eventRepresentation,
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

    public async Task<ProcessParameters> CreateAndExecuteAsync(Guid processContractId, ProcessParameters parameters,
        CancellationToken cancellationToken = default)
    {
        // TODO: Add parameter to control TAP awaiting processes with  asynchronous resume

        // Use scoped repository
        using var scope = _serviceProvider.CreateScope();
        var processContractRepository = scope.ServiceProvider.GetRequiredService<IProcessContractRepository>();
        var processContract =
            await processContractRepository.GetActiveProcessContractAsync(processContractId);

        if (processContract == null)
        {
            throw new Exception("processContract not exists");
        }

        return await ExecuteAsync(processContract, Guid.NewGuid(), parameters, true, cancellationToken);
    }

    public async Task<ProcessParameters> CreateAndExecuteAsync(Guid processContractId, Guid processVersionId,
        int versionNumber,
        ProcessParameters parameters,
        CancellationToken cancellationToken = default)
    {
        // TODO: Add parameter to control TAP awaiting processes with  asynchronous resume

        // Use scoped repository
        using var scope = _serviceProvider.CreateScope();
        var processContractRepository = scope.ServiceProvider.GetRequiredService<IProcessContractRepository>();
        var processContract =
            await processContractRepository.GetProcessContractAsync(processContractId, processVersionId,
                versionNumber);

        if (processContract == null)
        {
            throw new Exception("processContract not exists");
        }

        return await ExecuteAsync(processContract, Guid.NewGuid(), parameters, true, cancellationToken);
    }

    private async Task<ProcessParameters> ExecuteAsync(ProcessContract processContract, Guid processInstanceId,
        ProcessParameters parameters, bool createNew, CancellationToken cancellationToken = default)
    {
        // TODO: Add parameter to control TAP awaiting processes with  asynchronous resume

        var instance = await _driver.CreateInstanceAsync(processContract, processInstanceId, cancellationToken);
        _instances[instance.Id] = instance;
        var state = ProcessInstanceState.New;

        // use scoped repository
        var scope = _serviceProvider.CreateScope();
        var processInstanceRepository = scope.ServiceProvider.GetRequiredService<IProcessInstanceRepository>();
        if (instance.PersistenceType != PersistenceType.No && createNew)
        {
            await processInstanceRepository.CreateInstanceAsync(instance.Id,
                instance.ProcessContract.ProcessContractId, instance.ProcessContract.ProcessVersionId,
                instance.ProcessContract.VersionNumber, instance.PersistenceType, state, cancellationToken);
        }

        foreach (var (parameter, value) in parameters)
        {
            instance.ProcessRuntime.SetParameter(parameter, AsouTypes.UnSet, value);
        }

        try
        {
            state = ProcessInstanceState.Running;
            if (instance.PersistenceType != PersistenceType.No)
            {
                await processInstanceRepository.UpdateStateInstanceAsync(instance.Id, state, cancellationToken);
            }

            var result = await _driver.RunAsync(instance, cancellationToken);
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
            if (instance.PersistenceType != PersistenceType.No)
            {
                await processInstanceRepository.UpdateStateInstanceAsync(instance.Id, state, cancellationToken);
            }

            scope.Dispose();
        }
    }

    private async Task ResumeRunningProcessesAsync(CancellationToken cancellationToken = default)
    {
        // Use scoped repositories
        using var scope = _serviceProvider.CreateScope();
        var processInstanceRepository = scope.ServiceProvider.GetRequiredService<IProcessInstanceRepository>();
        var processContractRepository = scope.ServiceProvider.GetRequiredService<IProcessContractRepository>();
        var processExecutionPersistenceRepository =
            scope.ServiceProvider.GetRequiredService<IProcessExecutionPersistenceRepository>();
        var instances = await processInstanceRepository.GetARunningInstancesAsync(cancellationToken);
        var tasks = new List<Task<ProcessParameters>>();
        foreach (var instance in instances)
        {
            var processContract = await processContractRepository.GetProcessContractAsync(instance.ProcessContractId,
                instance.ProcessVersionId, instance.Version);
            if (processContract == null)
            {
                // TODO: Log warn event
                continue;
            }

            var parameters =
                await processExecutionPersistenceRepository.GetProcessParametersAsync(instance.Id, cancellationToken);

            // Execute without awaiting asynchronous task based operation
            var task = ExecuteAsync(processContract, instance.Id, new ProcessParameters(parameters), false,
                cancellationToken);
            tasks.Add(task);
        }
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
        using var scope = _serviceProvider.CreateScope();
        var processExecutionPersistenceRepository =
            scope.ServiceProvider.GetRequiredService<IProcessExecutionPersistenceRepository>();
        foreach (var (instanceId, instance) in _instances.Where(w => w.Value.PersistenceType != PersistenceType.No))
        {
            var parameters = instance.ProcessRuntime.Parameters.ToValueContainerMap();
            await processExecutionPersistenceRepository.StoreProcessParameterAsync(instanceId, parameters,
                cancellationToken);
        }
    }
}
