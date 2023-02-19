using Asou.Abstractions.Events;
using Microsoft.EntityFrameworkCore;

namespace Asou.EfCore.EventSubscription;

public class SubscriptionPersistantEfCoreRepository : ISubscriptionPersistantRepository
{
    private readonly DbContext _dbContext;
    private readonly DbSet<EventSubscriptionModel> _eventSubscription;

    public SubscriptionPersistantEfCoreRepository(DbContextResolver dbContextResolver)
    {
        _dbContext = dbContextResolver.GetContext();
        _eventSubscription = _dbContext.Set<EventSubscriptionModel>();
    }

    public async Task CreateSubscriptionAsync(EventSubscriptionModel eventSubscription,
        CancellationToken cancellationToken = default)
    {
        await _eventSubscription.AddAsync(eventSubscription, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DisableSubscriptionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscription = await _eventSubscription.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        if (subscription != null)
        {
            subscription.IsActive = false;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<EventSubscriptionModel>> GetApplicableSubscriptionsAsync(string source, string type,
        string subject,
        CancellationToken cancellationToken = default)
    {
        var subscriptions = await _eventSubscription
            .Where(w => w.Source == source || w.Source == string.Empty)
            .Where(w => w.Type == type || w.Type == string.Empty)
            .Where(w => w.Subject == subject || w.Subject == string.Empty)
            .Where(w => w.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return subscriptions;
    }

    public async Task<IEnumerable<EventSubscriptionModel>> GetActiveSubscriptionsAsync(
        CancellationToken cancellationToken = default)
    {
        var subscriptions = await _eventSubscription
            .Where(w => w.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return subscriptions;
    }
}
