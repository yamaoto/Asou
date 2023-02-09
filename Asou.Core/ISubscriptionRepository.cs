namespace Asou.Core;

public interface ISubscriptionRepository
{
    Task CreateSubscriptionAsync(EventSubscriptionModel eventSubscription,
        CancellationToken cancellationToken = default);

    Task DisableSubscriptionAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<EventSubscriptionModel>> GetApplicableSubscriptions(string source, string type, string subject,
        CancellationToken cancellationToken = default);
}