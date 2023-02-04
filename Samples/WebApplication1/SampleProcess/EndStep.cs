using Asou.Abstractions.ExecutionElements;

namespace WebApplication1.SampleProcess;

public class EndStep : BaseElement
{
    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}