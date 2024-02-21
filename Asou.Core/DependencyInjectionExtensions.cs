using Asou.Abstractions.Events;
using Asou.Core;
using Asou.Core.Commands;
using Asou.Core.Commands.Infrastructure;
using Asou.Core.Process.Binding;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    /// <summary>
    ///     Register ASOU Core services
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterAsouCoreServices(this IServiceCollection services)
    {
        services.TryAddSingleton<ProcessExecutionEngine>();
        services.TryAddSingleton<ISubscriptionManager, SubscriptionManager>();
        services.TryAddSingleton<IParameterDelegateFactory, ParameterDelegateFactory>();

        // Register CQRS runner
        services.AddTransient<ScopedActionRunner>();

        // Register CQRS commands and requests
        services.TryAddScoped<GetProcessesForResumeRequest>();
        services.TryAddScoped<GetProcessContractRequest>();
        services.TryAddScoped<StoreProcessParametersCommand>();
        services.TryAddScoped<UpdateProcessInstanceStateCommand>();
        services.TryAddScoped<InsertProcessInstanceStateCommand>();
        services.TryAddScoped<ResumeProcessesOnStartup>();

        return services;
    }
}
