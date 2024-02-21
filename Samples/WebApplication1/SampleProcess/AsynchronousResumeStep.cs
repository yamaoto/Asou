using Asou.Abstractions.Events;
using Asou.Abstractions.ExecutionElements;

namespace WebApplication1.SampleProcess;

public class AsynchronousResumeStep : BaseElement, IAfterExecution, IAsynchronousResume
{
    private readonly IEventBus _eventBus;

    public AsynchronousResumeStep(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public Task AfterExecutionAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<IEnumerable<EventSubscription>> ConfigureSubscriptionsAsync(
        CancellationToken cancellationToken = default)
    {
        // Subscribe to event with type MyEventType and subject MyEventSubject
        return Task.FromResult((IEnumerable<EventSubscription>)new[]
        {
            new EventSubscription("", "MyEventType", "MyEventSubject", EventSubscriptionType.AsynchronousResume)
        });
    }

    /// <summary>
    ///     Pass all events
    /// </summary>
    /// <param name="eventRepresentation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<bool> ValidateSubscriptionEventAsync(EventRepresentation eventRepresentation,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
