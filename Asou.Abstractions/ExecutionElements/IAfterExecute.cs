namespace Asou.Abstractions.ExecutionElements;

/// <summary>An interface for <see cref="BaseElement" /> that can be executed after a task finishes.</summary>
public interface IAfterExecute
{
    /// <summary>Executes after the <see cref="BaseElement" /> has been executed.</summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the operation.</returns>
    public Task AfterExecuteAsync(CancellationToken cancellationToken = default);
}