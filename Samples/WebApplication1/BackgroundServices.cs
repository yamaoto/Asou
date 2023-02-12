using Asou.Core;

namespace WebApplication1;

public class BackgroundServices : IHostedService
{
    private readonly InMemoryEventDriverWorker _inMemoryEventDriverWorker;

    public BackgroundServices(InMemoryEventDriverWorker inMemoryEventDriverWorker)
    {
        _inMemoryEventDriverWorker = inMemoryEventDriverWorker;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _inMemoryEventDriverWorker.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _inMemoryEventDriverWorker.Stop();
        return Task.CompletedTask;
    }
}
