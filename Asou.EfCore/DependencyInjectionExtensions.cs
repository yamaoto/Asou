using Asou.Abstractions.Repositories;
using Asou.EfCore;
using Asou.GraphEngine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection RegisterAsouEfCorePersistence<T>(this IServiceCollection services)
        where T : DbContext
    {
        services.TryAddScoped<DbContextResolver>(serviceResolver =>
            new DbContextResolver(serviceResolver.GetRequiredService<T>()));
        services.TryAddTransient<IProcessInstanceRepository, ProcessInstanceEfCoreRepository>();
        services.TryAddTransient<ISubscriptionPersistantRepository, SubscriptionPersistantEfCoreRepository>();
        services.TryAddTransient<IExecutionPersistence, ExecutionPersistence>();
        return services;
    }

    public static ModelBuilder RegisterAsouTypes(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventSubscriptionModelConfiguration());
        return modelBuilder;
    }
}
