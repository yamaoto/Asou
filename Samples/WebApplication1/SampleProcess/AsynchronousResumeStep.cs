using Asou.Abstractions;
using Asou.Abstractions.Events;
using Asou.Abstractions.ExecutionElements;
using Asou.Core;

namespace WebApplication1.SampleProcess;

public class AsynchronousResumeStep : BaseElement, IAfterExecution, IAsynchronousResume
{
    private readonly IEventDriver _eventDriver;

    public AsynchronousResumeStep(IEventDriver eventDriver)
    {
        _eventDriver = eventDriver;
    }

    public Task AfterExecutionAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<IEnumerable<EventSubscription>> ConfigureSubscriptionsAsync(
        CancellationToken cancellationToken = default)
    {
        // Subscribe to event with type MyEventTpe and subject MyEventSubject
        return Task.FromResult((IEnumerable<EventSubscription>)new[]
        {
            new EventSubscription("", "MyEventTpe", "MyEventSubject", EventSubscriptionType.AsynchronousResume)
        });
    }

    /// <summary>
    /// Pass all events
    /// </summary>
    /// <param name="eventRepresentation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<bool> ValidateSubscriptionEventAsync(EventRepresentation eventRepresentation,
        CancellationToken cancellationToken = default) => Task.FromResult(true);

    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var cts = new CancellationTokenSource();
        // Execute background task to simulate incoming event
        Task.Run(async () =>
        {
            Console.WriteLine("AsynchronousResumeStep 1 {0}", DateTime.Now);
            await Task.Delay(1000, cts.Token);
            Console.WriteLine("AsynchronousResumeStep 2 {0}", DateTime.Now);
            await _eventDriver.PublishAsync(
                new EventRepresentation(Guid.NewGuid().ToString(), "urn:WebApplication1.SampleProcess.AsynchronousResumeStep",
                    "MyEventTpe", "MyEventSubject", DateTime.Now, null), cts.Token);
        }, cts.Token);
        return Task.CompletedTask;
    }
}
