using System.Text.Json;
using Asou.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using NATS.Client;

namespace Asou.NatsMessaging;

public class NatsMessagingService : IMessagingService
{
    private readonly IConnection _connection;
    private readonly ILogger<NatsMessagingService> _logger;
    private readonly NatsMessagingChannel _natsMessagingChannel;

    private readonly Dictionary<long, IAsyncSubscription> _subscriptions = new();

    public NatsMessagingService(IConnection connection, ILogger<NatsMessagingService> logger,
        NatsMessagingChannel natsMessagingChannel)
    {
        _connection = connection;
        _logger = logger;
        _natsMessagingChannel = natsMessagingChannel;
        CurrentNode = Environment.MachineName;
    }

    public string CurrentNode { get; }

    public Task SendAddressedAsync<T>(string queue, T message, CancellationToken cancellationToken = default)
        where T : notnull
    {
        _connection.Publish(queue, JsonSerializer.SerializeToUtf8Bytes(message));
        return Task.CompletedTask;
    }

    public Task<object> SubscribeAsync(string queue, Type messageType, CancellationToken cancellationToken = default)
    {
        var subscription = _connection.SubscribeAsync(queue,
            (sender, args) => OnIncomingMessageHandler(args, queue, messageType));
        _subscriptions.Add(subscription.Sid, subscription!);
        return Task.FromResult((object)subscription);
    }

    public Task CancelSubscriptionAsync(string queue, object subscription,
        CancellationToken cancellationToken = default)
    {
        if (subscription is IAsyncSubscription asyncSubscription)
        {
            asyncSubscription.Unsubscribe();
            _subscriptions.Remove(asyncSubscription.Sid);
        }

        return Task.CompletedTask;
    }

    private void OnIncomingMessageHandler(MsgHandlerEventArgs args, string queueName, Type messageType)
    {
        var message = JsonSerializer.Deserialize(args.Message.Data, messageType);
        if (message == null)
        {
            _logger.LogError("Failed to deserialize message from {QueueName}", queueName);
            return;
        }

        var messageWrapper = new QueueMessage(queueName, message);
        while (!_natsMessagingChannel.ChannelWriter.TryWrite(messageWrapper))
        {
            _logger.LogWarning("Failed to write message to channel. Retrying...");
        }
    }

    public void Stop()
    {
        _logger.LogDebug("Stopping NatsMessagingService...");
        foreach (var subscription in _subscriptions.Values)
        {
            try
            {
                subscription.Unsubscribe();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to unsubscribe from queue");
            }
        }

        _logger.LogDebug("NatsMessagingService is stopped");
    }
}
