using Asou.Abstractions.Process.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asou.EfCore.ProcessPersistence;

public class ProcessExecutionLogModelConfiguration : IEntityTypeConfiguration<ProcessExecutionLogModel>
{
    public void Configure(EntityTypeBuilder<ProcessExecutionLogModel> builder)
    {
    }
}
