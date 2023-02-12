using Asou.Abstractions.ExecutionElements;

namespace WebApplication1.SampleProcess;

public class BindingStep : BaseElement
{
    public int? IntValue { get; set; }
    public string? StringValue { get; set; }
    public string? Result { get; set; }

    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        Result = IntValue + StringValue;
        return Task.CompletedTask;
    }
}
