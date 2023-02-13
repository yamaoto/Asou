using Asou.Abstractions.Process.Instance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asou.EfCore.ProcessInstance;

public class ProcessInstanceModelConfiguration : IEntityTypeConfiguration<ProcessInstanceModel>
{
    public void Configure(EntityTypeBuilder<ProcessInstanceModel> builder)
    {
    }
}
