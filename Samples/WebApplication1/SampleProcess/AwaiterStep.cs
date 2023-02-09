using Asou.Abstractions;
using Asou.Abstractions.Events;
using Asou.Abstractions.ExecutionElements;

namespace WebApplication1.SampleProcess;

public class AwaiterStep : BaseElement, IAfterExecute, IAsyncExecutionElement
{
    public Task AfterExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<IEnumerable<EventSubscription>> ConfigureSubscriptionsAsync(
        CancellationToken cancellationToken = default)
    {
        // subscribe to event with type MyEventTpe and subject MyEventSubject
        return Task.FromResult((IEnumerable<EventSubscription>)new[]
        {
            new EventSubscription("", "MyEventTpe", "MyEventSubject", EventSubscriptionType.AsyncExecutionResumer)
        });
    }

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