using Asou.Abstractions;
using Asou.Abstractions.Events;
using Asou.InMemoryEventDriver;
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
    public static IServiceCollection RegisterAsouInMemoryEventDriver(this IServiceCollection services)
    {
        services.TryAddSingleton<InMemoryEventDriverQueue>();
        services.TryAddTransient<IEventDriver, InMemoryEventDriver>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IInitializeHook, InMemoryEventDriverWorker>());
        return services;
    }
}
