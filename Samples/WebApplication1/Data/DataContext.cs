using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Add ASOU tables
        modelBuilder.RegisterAsouTypes();
    }
}
