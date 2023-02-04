using Asou.Abstractions.Events;

namespace Asou.Abstractions.ExecutionElements;

/// <summary>Configures the persistant awaiter for <see cref="BaseElement" />.</summary>
/// <param name="cancellationToken">The cancellation token.</param>
public interface IAsyncExecutionElement
{
    /// <summary>Configures the persistant awaiter.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Subsctipyions for target events</returns>
    public Task<IEnumerable<EventSubscription>> ConfigureSubscriptionsAsync(CancellationToken cancellationToken = default);
    
    public Task<bool> ValidateSubscriptionEventAsync(EventRepresentation eventRepresentation, CancellationToken cancellationToken = default);
}