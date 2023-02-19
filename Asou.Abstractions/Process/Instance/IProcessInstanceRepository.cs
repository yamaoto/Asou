namespace Asou.Abstractions.Process.Instance;

public interface IProcessInstanceRepository
{
    Task CreateInstanceAsync(Guid id, Guid processContractId, Guid processVersionId, int version,
        PersistenceType persistenceType, ProcessInstanceState state, CancellationToken cancellationToken = default);

    Task UpdateStateInstanceAsync(Guid id, ProcessInstanceState state, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProcessInstanceModel>> GetRunningInstancesAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<ProcessInstanceModel>> GetAllInstancesAsync(CancellationToken cancellationToken = default);
}
