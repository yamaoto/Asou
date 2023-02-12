using Asou.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asou.EfCore;

public class EventSubscriptionModelConfiguration : IEntityTypeConfiguration<EventSubscriptionModel>
{
    public void Configure(EntityTypeBuilder<EventSubscriptionModel> builder)
    {
    }
}
