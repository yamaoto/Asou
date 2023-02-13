using Asou.Abstractions.Events;

namespace Asou.InMemoryEventDriver;

public class InMemoryEventDriver : IEventDriver
{
    private readonly InMemoryEventDriverQueue _queue;
    private readonly ISubscriptionPersistantRepository _subscriptionPersistantRepository;

    public InMemoryEventDriver(
        InMemoryEventDriverQueue queue,
        ISubscriptionPersistantRepository subscriptionPersistantRepository
    )
    {
        _queue = queue;
        _subscriptionPersistantRepository = subscriptionPersistantRepository;
    }

    public Task PublishAsync(EventRepresentation eventRepresentation, CancellationToken cancellationToken = default)
    {
        _queue.Enqueue(eventRepresentation);
        return Task.CompletedTask;
    }

    public async Task<Guid> SubscribeAsync(Guid instanceId, Guid threadId, Guid elementId,
        EventSubscription eventSubscription,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        await _subscriptionPersistantRepository.CreateSubscriptionAsync(new EventSubscriptionModel(id,
            instanceId, threadId, elementId, eventSubscription.Source, eventSubscription.Type,
            eventSubscription.Subject, eventSubscription.EventSubscriptionType), cancellationToken);
        return id;
    }

    public async Task CancelSubscriptionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _subscriptionPersistantRepository.DisableSubscriptionAsync(id, cancellationToken);
    }
}
