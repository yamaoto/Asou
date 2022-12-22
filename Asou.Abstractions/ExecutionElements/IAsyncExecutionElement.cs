namespace Asou.Abstractions.ExecutionElements;

public interface IAsyncExecutionElement
{
    public Task ConfigureAwaiterAsync(CancellationToken cancellationToken = default);
}