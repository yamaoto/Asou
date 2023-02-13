using Asou.Abstractions.Events;
using Asou.Abstractions.Process.Execution;
using Asou.Abstractions.Process.Instance;
using Asou.EfCore;
using Asou.EfCore.EventSubscription;
using Asou.EfCore.ProcessInstance;
using Asou.EfCore.ProcessPersistence;
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
        services.TryAddScoped<IProcessInstanceRepository, ProcessInstanceEfCoreRepository>();
        services.TryAddScoped<ISubscriptionPersistantRepository, SubscriptionPersistantEfCoreRepository>();
        services.TryAddScoped<IProcessExecutionLogRepository, ProcessExecutionLogEfCoreRepository>();
        services.TryAddScoped<IProcessExecutionPersistenceRepository, ProcessExecutionPersistenceEfCoreRepository>();
        return services;
    }

    public static ModelBuilder RegisterAsouTypes(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventSubscriptionModelConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessInstanceModelConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessExecutionLogModelConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessParameterPersistentModelConfiguration());
        return modelBuilder;
    }
}
