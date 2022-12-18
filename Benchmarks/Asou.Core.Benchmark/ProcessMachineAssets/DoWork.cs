using Asou.Core.Abstractions.ExecutionElements;

namespace Asou.Core.Benchmark.ProcessMachineAssets;

internal class DoWork : BaseElement
{
    public override string ClassName { get; init; } = nameof(DoWork);

    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}