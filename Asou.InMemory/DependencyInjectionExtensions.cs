using Asou.Abstractions;
using Asou.Abstractions.Distributed;
using Asou.Abstractions.Events;
using Asou.InMemory;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    /// <summary>
    ///     Register ASOU InMemory event driver
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterAsouInMemory(this IServiceCollection services)
    {
        services.TryAddSingleton<InMemoryEventDriverQueue>();
        services.TryAddTransient<IEventBus, InMemoryEventBus>();
        services.TryAddTransient<ILeaderElectionService, InMemoryLeaderElection>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IInitializeHook, InMemoryEventDriverWorker>());
        return services;
    }
}
