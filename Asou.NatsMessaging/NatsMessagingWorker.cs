using Asou.Abstractions;
using Asou.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NATS.Client;

namespace Asou.NatsMessaging;

public class NatsMessagingWorker : IInitializeHook, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IConnection _connection;
    private readonly ILogger<NatsMessagingWorker> _logger;
    private readonly IMessagingService _messagingService;
    private readonly NatsMessagingChannel _natsMessagingChannel;
    private readonly MessageConsumerRegistrations _registrations;
    private readonly IServiceProvider _serviceProvider;
    private Task? _workerTask;

    public NatsMessagingWorker(
        NatsMessagingChannel natsMessagingChannel,
        IServiceProvider serviceProvider,
        MessageConsumerRegistrations registrations,
        ILogger<NatsMessagingWorker> logger,
        IConnection connection,
        IMessagingService messagingService)
    {
        _natsMessagingChannel = natsMessagingChannel;
        _serviceProvider = serviceProvider;
        _registrations = registrations;
        _logger = logger;
        _connection = connection;
        _messagingService = messagingService;
    }

    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
        _workerTask?.Dispose();
    }

    public Task Initialize(CancellationToken cancellationToken = default)
    {
        _workerTask = HandleEvents();
        return Task.CompletedTask;
    }

    public void Stop()
    {
        // TODO: Add stop invocation from IInitializeHook
        _logger.LogDebug("Stopping NatsConnectionFactory...");
        _cancellationTokenSource.Cancel();
        if (_messagingService is NatsMessagingService natsMessagingService)
        {
            natsMessagingService.Stop();
        }

        _connection.Close();
        _logger.LogDebug("NatsConnectionFactory is stopped");
    }

    private async Task HandleEvents()
    {
        var registrations = _registrations.GetRegistrations();
        var messages = _natsMessagingChannel.ChannelReader.ReadAllAsync(_cancellationTokenSource.Token);
        await foreach (var message in messages)
        {
            if (!registrations.ContainsKey(message.Queue))
            {
                _logger.LogError("Received unsupported queue message from {QueueName}", message.Queue);
                continue;
            }

            var scope = _serviceProvider.CreateScope();
            var handler = (IMessageConsumer)scope.ServiceProvider.GetRequiredService(registrations[message.Queue]);
            try
            {
                await handler.HandleMessageAsync(message.Message, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Unhandled exception thrown in message handler. See inner exception details");
            }

            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
