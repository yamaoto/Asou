namespace Asou.Abstractions.ExecutionElements;

public interface IAfterExecute
{
    public Task AfterExecuteAsync(CancellationToken cancellationToken = default);
}