using Asou.Abstractions.Events;

namespace Asou.NatsMessaging;

/// <summary>
///     TODO: Split code and move _subscriptionPersistantRepository calling into SubscriptionManager
/// </summary>
public class NatsEventBus : IEventBus
{
    public string CurrentNode { get; }

    public async Task SendAddressedAsync(string nodeName, EventRepresentation eventRepresentation,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task SendAsync(EventRepresentation eventRepresentation, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task PublishAsync(EventRepresentation eventRepresentation,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Guid> SubscribeAsync(Guid instanceId, Guid threadId, Guid elementId,
        EventSubscription eventSubscription,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task CancelSubscriptionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
