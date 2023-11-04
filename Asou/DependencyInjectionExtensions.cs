using Asou;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAsou<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        // Register ASOU Core services
        services.RegisterAsouCoreServices();

        // Register ASOU Graph engine
        services.RegisterAsouGraphEngine();

        // Register ASOU in-memory & singlenode mode
        services.RegisterAsouInMemory();

        // Use EF Core persistence provider
        services.RegisterAsouEfCorePersistence<TDbContext>();

        services.AddHostedService<AsouBackgroundServices>();
        return services;
    }
}
