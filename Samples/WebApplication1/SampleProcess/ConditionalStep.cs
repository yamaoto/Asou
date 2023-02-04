using Asou.Abstractions.ExecutionElements;

namespace WebApplication1.SampleProcess;

public class ConditionalStep : BaseElement, IPreconfiguredConditions
{
    public bool CheckCondition(string name)
    {
        return name switch
        {
            "ToExit" => true,
            "TryAgain" => false,
            _ => false
        };
    }

    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}