using Asou;
using Asou.Abstractions.Events;
using Asou.Core;
using Asou.Core.Process.Binding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAsou<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        // Register ASOU core services
        services.TryAddSingleton<ISubscriptionManager, SubscriptionManager>();
        services.TryAddSingleton<ProcessExecutionEngine>();
        services.TryAddSingleton<IParameterDelegateFactory, ParameterDelegateFactory>();

        // Add ASOU engine
        services.RegisterAsouGraphEngine();

        // Add ASOU InMemory event driver
        services.RegisterAsouInMemoryEventDriver();

        // Use EF Core persistence provider
        services.RegisterAsouEfCorePersistence<TDbContext>();

        services.AddHostedService<AsouBackgroundServices>();
        return services;
    }
}
