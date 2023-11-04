namespace Asou.Abstractions.Events;

/// <summary>
///     Event driver for publishing and subscribing to events.
///     TODO: Split into IEventBus and IMessagingService, where IMessagingService will expose low level messaging/queue API
///     and IEventBus will work on top of it.
/// </summary>
public interface IEventBus
{
    public string CurrentNode { get; }

    /// <summary>
    ///     Send event to a specific node.
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="eventRepresentation">Event representation</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendAddressedAsync(string nodeName, EventRepresentation eventRepresentation,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Send event for shared queue
    /// </summary>
    /// <param name="eventRepresentation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendAsync(EventRepresentation eventRepresentation, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Send event to all nodes.
    /// </summary>
    /// <param name="eventRepresentation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishAsync(EventRepresentation eventRepresentation, CancellationToken cancellationToken = default);

    Task<Guid> SubscribeAsync(Guid instanceId, Guid threadId, Guid elementId, EventSubscription eventSubscription,
        CancellationToken cancellationToken = default);

    Task CancelSubscriptionAsync(Guid id, CancellationToken cancellationToken = default);
}
