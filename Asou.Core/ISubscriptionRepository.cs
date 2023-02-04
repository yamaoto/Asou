namespace Asou.Core;

public interface ISubscriptionRepository
{
    Task<IEnumerable<EventSubscriptionModel>> GetActiveSubscriptions(Guid processInstanceId, CancellationToken cancellationToken = default);
    Task CreateSubscriptionAsync(EventSubscriptionModel eventSubscription, CancellationToken cancellationToken = default);
    Task DisableSubscriptionAsync(Guid id, CancellationToken cancellationToken = default);
}
