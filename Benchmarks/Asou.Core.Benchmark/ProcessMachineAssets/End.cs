using Asou.Abstractions.ExecutionElements;

namespace Asou.Core.Benchmark.ProcessMachineAssets;

internal sealed class End : BaseElement
{
    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
