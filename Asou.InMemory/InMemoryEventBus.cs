using Asou.Abstractions.Events;
using Asou.Abstractions.Messaging;

namespace Asou.InMemory;

public class InMemoryEventBus : IEventBus
{
    private static readonly Dictionary<Guid, string> _eventSubscriptions = new();
    private static readonly Dictionary<string, (object subscription, int count)> _queueSubscriptions = new();
    private readonly IMessagingService _messagingService;
    private readonly ISubscriptionPersistantRepository _subscriptionPersistantRepository;

    public InMemoryEventBus(
        IMessagingService messagingService,
        ISubscriptionPersistantRepository subscriptionPersistantRepository
    )
    {
        _messagingService = messagingService;
        _subscriptionPersistantRepository = subscriptionPersistantRepository;
    }

    public async Task SendAddressedAsync(string nodeName, EventRepresentation eventRepresentation,
        CancellationToken cancellationToken = default)
    {
        await _messagingService.SendAddressedAsync(string.Intern($"{nodeName}.Event.{eventRepresentation.Type}"),
            eventRepresentation,
            cancellationToken);
    }

    public async Task
        SendAsync(EventRepresentation eventRepresentation, CancellationToken cancellationToken = default)
    {
        await _messagingService.SendAddressedAsync(string.Intern($"Shared.Event.{eventRepresentation.Type}"),
            eventRepresentation,
            cancellationToken);
    }

    public async Task PublishAsync(EventRepresentation eventRepresentation,
        CancellationToken cancellationToken = default)
    {
        await _messagingService.SendAddressedAsync("All.Event", eventRepresentation, cancellationToken);
    }

    public async Task<Guid> SubscribeAsync(Guid instanceId, Guid threadId, Guid elementId,
        EventSubscription eventSubscription,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        var queue = string.Intern($"{CurrentNode}.Event.{eventSubscription.Type}");
        if (_queueSubscriptions.TryGetValue(queue, out var value))
        {
            value.count++;
        }
        else
        {
            var messageSubscription =
                await _messagingService.SubscribeAsync(queue, typeof(EventRepresentation), cancellationToken);
            _queueSubscriptions.Add(queue, (messageSubscription, 1));
        }

        // TODO: Split code and move _subscriptionPersistantRepository calling into SubscriptionManager
        await _subscriptionPersistantRepository.CreateSubscriptionAsync(new EventSubscriptionModel(id,
            instanceId, threadId, elementId, eventSubscription.Source, eventSubscription.Type,
            eventSubscription.Subject, eventSubscription.EventSubscriptionType), cancellationToken);
        _eventSubscriptions.Add(id, queue);
        return id;
    }

    public async Task CancelSubscriptionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // TODO: Split code and move _subscriptionPersistantRepository calling into SubscriptionManager
        await _subscriptionPersistantRepository.DisableSubscriptionAsync(id, cancellationToken);
        if (_eventSubscriptions.TryGetValue(id, out var queue))
        {
            if (_queueSubscriptions.TryGetValue(queue, out var value))
            {
                value.count--;
                if (value.count == 0)
                {
                    await _messagingService.CancelSubscriptionAsync(queue, value.subscription, cancellationToken);
                    _queueSubscriptions.Remove(queue);
                }
            }

            _eventSubscriptions.Remove(id);
        }
    }

    public string CurrentNode => _messagingService.CurrentNode;
}
