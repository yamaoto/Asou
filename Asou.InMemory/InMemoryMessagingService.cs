using Asou.Abstractions.Messaging;

namespace Asou.InMemory;

public class InMemoryMessagingService : IMessagingService
{
    private static readonly object _stubObject = new();
    private readonly InMemoryMessageQueue _queue;

    public InMemoryMessagingService(InMemoryMessageQueue queue)
    {
        _queue = queue;
        CurrentNode = "InMemory";
    }

    public string CurrentNode { get; }

    public Task SendAddressedAsync<T>(string queue, T message, CancellationToken cancellationToken = default)
        where T : notnull
    {
        _queue.Enqueue(new QueueMessage(queue, message));
        return Task.CompletedTask;
    }

    public Task<object> SubscribeAsync(string queue, Type messageType, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_stubObject);
    }

    public Task CancelSubscriptionAsync(string queue, object subscription,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
