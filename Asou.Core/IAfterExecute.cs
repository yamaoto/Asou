namespace Asou.Core;

public interface IAfterExecute
{
    public Task AfterExecuteAsync(CancellationToken cancellationToken = default);
}