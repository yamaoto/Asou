using Asou.Abstractions;
using Asou.Abstractions.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Asou.InMemory;

public class InMemoryMessagingWorker : IInitializeHook, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly InMemoryMessageQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private Task? _workerTask;

    public InMemoryMessagingWorker(
        InMemoryMessageQueue queue,
        IServiceProvider serviceProvider
    )
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
    }

    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
        _workerTask?.Dispose();
    }

    public Task Initialize(CancellationToken cancellationToken = default)
    {
        Start();
        return Task.CompletedTask;
    }

    public void Start()
    {
        // Start background handler task
        _workerTask = Task.Run(HandleEvents, _cancellationTokenSource.Token);
    }

    public void Stop()
    {
        // Stop background task with cancellation token
        _cancellationTokenSource.Cancel();
    }

    private async void HandleEvents()
    {
        // loop while cancellation token isn't requested
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            QueueMessage message;
            try
            {
                message = await _queue.DequeueAsync(_cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Ignore TaskCanceledException
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var subscriptionManager = scope.ServiceProvider.GetRequiredService<ISubscriptionManager>();
            if (message.Message is EventRepresentation eventRepresentation)
            {
                // Call event system to check subscriptions and pass event
                await subscriptionManager.ReceiveEventAsync(eventRepresentation, _cancellationTokenSource.Token);
            }
        }
    }
}
