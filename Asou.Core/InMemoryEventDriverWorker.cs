using Microsoft.Extensions.DependencyInjection;

namespace Asou.Core;

public class InMemoryEventDriverWorker : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly InMemoryEventDriverQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private Task? _workerTask;

    public InMemoryEventDriverWorker(
        InMemoryEventDriverQueue queue,
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
            var message = await _queue.DequeueAsync(_cancellationTokenSource.Token);
            using var scope = _serviceProvider.CreateScope();
            var subscriptionManager = scope.ServiceProvider.GetRequiredService<SubscriptionManager>();
            // Call event system to check subscriptions and pass event
            await subscriptionManager.ReceiveEventAsync(message, _cancellationTokenSource.Token);
        }
    }
}
