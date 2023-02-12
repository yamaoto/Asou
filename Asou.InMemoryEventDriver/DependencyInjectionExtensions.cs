using Asou.Abstractions;
using Asou.Abstractions.Events;
using Asou.InMemoryEventDriver;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection RegisterAsouInMemoryEventDriver(this IServiceCollection services)
    {
        services.TryAddSingleton<InMemoryEventDriverQueue>();
        services.TryAddTransient<IEventDriver, InMemoryEventDriver>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IInitializeHook, InMemoryEventDriverWorker>());
        return services;
    }
}
