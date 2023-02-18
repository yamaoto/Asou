namespace Asou.Abstractions.Process.Execution;

public interface IProcessExecutionLogRepository
{
    Task SaveExecutionStatusAsync(Guid processContractId, Guid processVersionId, Guid processInstanceId,
        Guid threadId, Guid elementId, int state, CancellationToken cancellationToken = default);

    Task FlushLogForProcessInstanceAsync(Guid processInstanceId, CancellationToken cancellationToken = default);

    Task<IEnumerable<ProcessExecutionLogModel>> GetLogForProcessInstanceAsync(Guid processInstanceId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ProcessExecutionLogModel>> GetThreadsAsync(Guid processInstanceId,
        CancellationToken cancellationToken = default);
}
