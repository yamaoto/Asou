using Asou.Abstractions;
using Asou.Abstractions.Events;

namespace Asou.Core;

// TODO: InMemory inheritor
// TODO: Mass transit inheritor
public interface IEventDriver
{
    Task PublishAsync(EventRepresentation eventRepresentation, CancellationToken cancellationToken = default);

    Task<Guid> SubscribeAsync(Guid instanceId, string elementName, EventSubscription eventSubscription, CancellationToken cancellationToken = default);
    Task CancelSubscriptionAsync(Guid id, CancellationToken cancellationToken = default);
}