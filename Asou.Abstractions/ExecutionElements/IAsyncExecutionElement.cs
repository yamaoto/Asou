using Asou.Abstractions.Events;

namespace Asou.Abstractions.ExecutionElements;

/// <summary>Configures the persistant awaiter for <see cref="BaseElement" />.</summary>
/// <param name="cancellationToken">The cancellation token.</param>
public interface IAsyncExecutionElement
{
    /// <summary>Configures the persistant awaiter.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the operation.</returns>
    public Task<IEnumerable<EventSubscription>> ConfigureAwaiterAsync(CancellationToken cancellationToken = default);
}