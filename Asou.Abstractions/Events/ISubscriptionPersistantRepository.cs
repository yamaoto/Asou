namespace Asou.Abstractions.Events;

public interface ISubscriptionPersistantRepository
{
    Task CreateSubscriptionAsync(EventSubscriptionModel eventSubscription,
        CancellationToken cancellationToken = default);

    Task DisableSubscriptionAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<EventSubscriptionModel>> GetApplicableSubscriptionsAsync(string source, string type,
        string subject,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<EventSubscriptionModel>>
        GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default);
}
