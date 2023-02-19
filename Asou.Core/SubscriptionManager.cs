using Asou.Abstractions.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Asou.Core;

public class SubscriptionManager : ISubscriptionManager
{
    private readonly ProcessExecutionEngine _processExecutionEngine;
    private readonly IServiceProvider _serviceProvider;

    public SubscriptionManager(
        ProcessExecutionEngine processExecutionEngine,
        IServiceProvider serviceProvider
    )
    {
        _processExecutionEngine = processExecutionEngine;
        _serviceProvider = serviceProvider;
    }

    public async Task ReceiveEventAsync(EventRepresentation eventRepresentation,
        CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var subscriptionRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionPersistantRepository>();
        var subscriptions = await subscriptionRepository
            .GetApplicableSubscriptionsAsync(eventRepresentation.Source, eventRepresentation.Type,
                eventRepresentation.Subject, cancellationToken);
        foreach (var subscription in subscriptions)
        {
            await _processExecutionEngine.HandleEventAsync(subscription, eventRepresentation,
                cancellationToken);
        }
    }
}
