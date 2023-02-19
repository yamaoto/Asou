using Asou.Abstractions.ExecutionElements;

namespace Asou.Core.Tests;

public sealed class TestElement : BaseElement
{
    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
