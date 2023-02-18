using Asou.Abstractions.ExecutionElements;

namespace WebApplication1.SampleProcess;

public class ConditionalStep : BaseElement, IPreconfiguredConditions
{
    public string? Parameter2 { get; set; }

    public bool CheckCondition(string name)
    {
        return name switch
        {
            "ToExit" => Parameter2 == "Hello World",
            "TryAgain" => Parameter2 != "Hello World",
            _ => false
        };
    }

    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
