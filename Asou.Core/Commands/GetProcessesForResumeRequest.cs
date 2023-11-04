using Asou.Abstractions.Container;
using Asou.Abstractions.Process.Contract;
using Asou.Abstractions.Process.Execution;
using Asou.Abstractions.Process.Instance;

namespace Asou.Core.Commands;

public class GetProcessesForResumeRequest
{
    private readonly IProcessContractRepository _processContractRepository;
    private readonly IProcessExecutionPersistenceRepository _processExecutionPersistenceRepository;
    private readonly IProcessInstanceRepository _processInstanceRepository;


    public GetProcessesForResumeRequest(IProcessInstanceRepository processInstanceRepository,
        IProcessContractRepository processContractRepository,
        IProcessExecutionPersistenceRepository processExecutionPersistenceRepository)
    {
        _processInstanceRepository = processInstanceRepository;
        _processContractRepository = processContractRepository;
        _processExecutionPersistenceRepository = processExecutionPersistenceRepository;
    }

    /// <summary>
    ///     Prepare data for resuming process
    /// </summary>
    /// <param name="processInstanceId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ResumeProcessRecord?> GetProcessToResumeAsync(Guid processInstanceId,
        CancellationToken cancellationToken = default)
    {
        var instance = await _processInstanceRepository.GetInstanceAsync(processInstanceId, cancellationToken);
        if (instance == null)
        {
            return null;
        }

        var (processContract, parameters) = await PrepareInstanceDataAsync(cancellationToken, instance);
        return new ResumeProcessRecord(instance, processContract, parameters);
    }

    /// <summary>
    ///     Prepare data for resuming processes
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ProcessInstanceModel>> GetAsyncEnumerable(
        CancellationToken cancellationToken = default)
    {
        var instances = await _processInstanceRepository.GetRunningInstancesAsync(cancellationToken);
        return instances;
    }

    /// <summary>
    ///     Fill process instance with data from database.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="instance"></param>
    /// <returns></returns>
    private async Task<(ProcessContract? processContract, Dictionary<string, ValueContainer> parameters)>
        PrepareInstanceDataAsync(CancellationToken cancellationToken, ProcessInstanceModel instance)
    {
        var processContract = await _processContractRepository.GetProcessContractAsync(instance.ProcessContractId,
            instance.ProcessVersionId, instance.Version);
        var parameters =
            await _processExecutionPersistenceRepository.GetProcessParametersAsync(instance.Id, cancellationToken);
        return (processContract, parameters);
    }
}
