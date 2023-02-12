using Asou.Core;
using Asou.GraphEngine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Asou.EfCore;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection RegisterAsouEfCorePersistence<T>(this IServiceCollection services)
        where T : DbContext
    {
        services.AddScoped<DbContextResolver>(serviceResolver =>
            new DbContextResolver(serviceResolver.GetRequiredService<T>()));
        services.AddTransient<IProcessInstanceRepository, ProcessInstanceEfCoreRepository>();
        services.AddTransient<ISubscriptionPersistantRepository, SubscriptionPersistantEfCoreRepository>();
        services.AddTransient<IExecutionPersistence, ExecutionPersistence>();
        return services;
    }

    public static ModelBuilder RegisterAsouTypes(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventSubscriptionModelConfiguration());
        return modelBuilder;
    }
}
