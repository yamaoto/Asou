using Asou.Abstractions.ExecutionElements;

namespace WebApplication1.UnitTests;

public sealed class TestElement : BaseElement
{
    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
