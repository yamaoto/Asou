namespace Asou.Abstractions.ExecutionElements;

/// <summary>Base class for all elements.</summary>
public abstract class BaseElement
{
    /// <summary>Executes the command.</summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the operation.</returns>
    public abstract Task ExecuteAsync(CancellationToken cancellationToken = default);
}
