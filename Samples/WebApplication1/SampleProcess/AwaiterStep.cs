using Asou.Abstractions.ExecutionElements;

namespace WebApplication1.SampleProcess;

public class AwaiterStep : BaseElement, IAfterExecute, IAsyncExecutionElement
{
    public override string ClassName { get; init; } = nameof(AwaiterStep);


    public Task AfterExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task ConfigureAwaiterAsync(CancellationToken cancellationToken = default)
    {
        // subscribe to events
        return Task.CompletedTask;
    }

    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}