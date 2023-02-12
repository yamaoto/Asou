using Asou.Abstractions.ExecutionElements;

namespace Asou.Core.Benchmark.ProcessMachineAssets;

internal sealed class Start : BaseElement
{
    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
