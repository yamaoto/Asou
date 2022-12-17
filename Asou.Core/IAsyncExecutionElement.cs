namespace Asou.Core;

public interface IAsyncExecutionElement
{
    public Task ConfigureAwaiterAsync(CancellationToken cancellationToken = default);
}