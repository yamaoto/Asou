namespace Asou.Core.Abstractions.ExecutionElements;

public interface IAsyncExecutionElement
{
    public Task ConfigureAwaiterAsync(CancellationToken cancellationToken = default);
}