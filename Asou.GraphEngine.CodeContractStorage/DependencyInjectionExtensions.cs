using Asou.Abstractions;
using Asou.Abstractions.Process;
using Asou.Abstractions.Repositories;
using Asou.GraphEngine;
using Asou.GraphEngine.CodeContractStorage;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection RegisterAsouGraphEngine(this IServiceCollection services)
    {
        services.TryAddTransient<IGraphProcessFactory, GraphProcessFactory>();
        services.TryAddTransient<IProcessFactory>(serviceProvider =>
            serviceProvider.GetRequiredService<IGraphProcessFactory>());
        services.TryAddSingleton<IGraphProcessContractRepository, GraphProcessContractRepository>();
        services.TryAddSingleton<IProcessContractRepository>(serviceProvider =>
            serviceProvider.GetRequiredService<IGraphProcessContractRepository>());
        services.TryAddSingleton<IProcessExecutionDriver, GraphProcessExecutionDriver>();
        services.TryAddTransient<IGraphProcessRegistration, GraphProcessRegistration>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IInitializeHook, InitializeCodeContractStorage>());

        return services;
    }

    public static IServiceCollection AddAsouProcess<T>(this IServiceCollection services)
        where T : class, IProcessDefinition
    {
        services.TryAddEnumerable(ServiceDescriptor.Transient<IProcessDefinition, T>());

        return services;
    }
}
