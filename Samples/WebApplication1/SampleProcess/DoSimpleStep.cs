using Asou.Abstractions.ExecutionElements;

namespace WebApplication1.SampleProcess;

public class DoSimpleStep : BaseElement
{
    public string? Parameter1 { get; set; }
    public string? Parameter2 { get; set; }

    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        Parameter2 = Parameter1;
        Parameter1 = null;
        return Task.CompletedTask;
    }
}
