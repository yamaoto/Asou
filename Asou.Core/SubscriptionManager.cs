using Asou.Abstractions;

namespace Asou.Core;

public class SubscriptionManager
{
    private readonly ProcessExecutionEngine _processExecutionEngine;
    private readonly ISubscriptionPersistantRepository _subscriptionRepository;

    public SubscriptionManager(
        ISubscriptionPersistantRepository subscriptionRepository,
        ProcessExecutionEngine processExecutionEngine
    )
    {
        _subscriptionRepository = subscriptionRepository;
        _processExecutionEngine = processExecutionEngine;
    }

    public async Task ReceiveEventAsync(EventRepresentation eventRepresentation,
        CancellationToken cancellationToken = default)
    {
        var subscriptions = await _subscriptionRepository
            .GetApplicableSubscriptionsAsync(eventRepresentation.Source, eventRepresentation.Type,
                eventRepresentation.Subject, cancellationToken);
        foreach (var subscription in subscriptions)
        {
            await _processExecutionEngine.HandleEventAsync(subscription, eventRepresentation,
                cancellationToken);
        }
    }
}
