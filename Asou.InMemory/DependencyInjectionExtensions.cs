using Asou.Abstractions;
using Asou.Abstractions.Distributed;
using Asou.Abstractions.Events;
using Asou.Abstractions.Messaging;
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
        services.TryAddSingleton<InMemoryMessageQueue>();
        services.TryAddTransient<IEventBus, InMemoryEventBus>();
        services.TryAddTransient<IMessagingService, MessagingService>();
        services.TryAddTransient<ILeaderElectionService, InMemoryLeaderElection>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IInitializeHook, InMemoryMessagingWorker>());
        return services;
    }
}
