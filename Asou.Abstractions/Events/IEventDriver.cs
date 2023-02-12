namespace Asou.Abstractions.Events;

// TODO: Mass transit inheritor
public interface IEventDriver
{
    Task PublishAsync(EventRepresentation eventRepresentation, CancellationToken cancellationToken = default);

    Task<Guid> SubscribeAsync(Guid instanceId, Guid threadId, Guid elementId, EventSubscription eventSubscription,
        CancellationToken cancellationToken = default);

    Task CancelSubscriptionAsync(Guid id, CancellationToken cancellationToken = default);
}
