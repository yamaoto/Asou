using Asou.Abstractions.Events;
using Asou.Abstractions.Messaging;
using Asou.NatsMessaging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NATS.Client;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    /// <summary>
    ///     Register ASOU NATS event driver
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureNats"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterAsouNatsMessaging(this IServiceCollection services,
        Action<NATS.Client.Options, NatsOptions>? configureNats = null)
    {
        services.TryAddTransient<IEventBus, NatsEventBus>();
        services.TryAddTransient<IMessagingService, NatsMessagingService>();
        services.AddSingleton<NATS.Client.Options>(serviceProvider =>
        {
            var natsOptions = serviceProvider.GetRequiredService<IOptions<NatsOptions>>();
            var options = ConnectionFactory.GetDefaultOptions();
            options.MaxReconnect = natsOptions.Value.MaxReconnect;
            options.ReconnectWait = natsOptions.Value.ReconnectWait;
            options.Servers = natsOptions.Value.Servers;
            configureNats?.Invoke(options, natsOptions.Value);
            return options;
        });
        services.AddSingleton<INatsConnectionFactory, NatsConnectionFactory>();
        services.AddSingleton<IConnection>(serviceProvider =>
            serviceProvider.GetRequiredService<INatsConnectionFactory>().GetConnection());
        return services;
    }
}
