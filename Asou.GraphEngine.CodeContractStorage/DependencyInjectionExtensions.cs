using Asou.Core;
using Asou.Core.Process.Binding;
using Microsoft.Extensions.DependencyInjection;

namespace Asou.GraphEngine.CodeContractStorage;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection RegisterAsouGraphEngine(this IServiceCollection services)
    {
        services.AddTransient<IGraphProcessFactory, GraphProcessFactory>();
        services.AddTransient<IProcessFactory>(serviceProvider =>
            serviceProvider.GetRequiredService<IGraphProcessFactory>());
        services.AddSingleton<IGraphProcessContractRepository, GraphProcessContractRepository>();
        services.AddSingleton<IProcessContractRepository>(serviceProvider =>
            serviceProvider.GetRequiredService<IGraphProcessContractRepository>());
        services.AddSingleton<IProcessExecutionDriver, GraphProcessExecutionDriver>();
        services.AddSingleton<ProcessExecutionEngine>();
        services.AddSingleton<IParameterDelegateFactory, ParameterDelegateFactory>();
        services.AddTransient<IGraphProcessRegistration, GraphProcessRegistration>();
        services.AddTransient<SubscriptionManager>();

        // TODO: Move to another assembly
        services.AddSingleton<InMemoryEventDriverWorker>();
        services.AddSingleton<InMemoryEventDriverQueue>();
        services.AddTransient<IEventDriver, InMemoryEventDriver>();
        return services;
    }
}
