namespace Asou.Core.Abstractions.ExecutionElements;

public interface IAfterExecute
{
    public Task AfterExecuteAsync(CancellationToken cancellationToken = default);
}