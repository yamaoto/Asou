using Asou.Abstractions.Process.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asou.EfCore.ProcessPersistence;

public class ProcessParameterPersistentModelConfiguration : IEntityTypeConfiguration<ProcessParameterPersistentModel>
{
    public void Configure(EntityTypeBuilder<ProcessParameterPersistentModel> builder)
    {
        builder.HasKey(k => k.ProcessInstanceId);
    }
}
