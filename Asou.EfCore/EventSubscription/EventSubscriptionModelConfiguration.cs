using Asou.Abstractions.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asou.EfCore.EventSubscription;

public class EventSubscriptionModelConfiguration : IEntityTypeConfiguration<EventSubscriptionModel>
{
    public void Configure(EntityTypeBuilder<EventSubscriptionModel> builder)
    {
    }
}
