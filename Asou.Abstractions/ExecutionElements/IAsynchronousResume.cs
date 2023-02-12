using Asou.Abstractions.Events;

namespace Asou.Abstractions.ExecutionElements;

/// <summary>Configures the continuous operation with asynchronous resume for <see cref="BaseElement" />.</summary>
/// <param name="cancellationToken">The cancellation token.</param>
public interface IAsynchronousResume
{
    /// <summary>Configures events to subscribe to wait asynchronous resume</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Subscriptions for target events</returns>
    public Task<IEnumerable<EventSubscription>> ConfigureSubscriptionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check events for applicability. Use this method to check applicability with event payload
    /// </summary>
    /// <param name="eventRepresentation">Event representation</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns></returns>
    public Task<bool> ValidateSubscriptionEventAsync(EventRepresentation eventRepresentation,
        CancellationToken cancellationToken = default);
}
