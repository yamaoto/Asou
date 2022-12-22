using Asou.Abstractions.ExecutionElements;

namespace Asou.Core.Benchmark.ParameterBinderAssets;

public class TestObject : BaseElement
{
    public string Value { get; set; } = "initial";

    public override string ClassName { get; init; } = nameof(TestObject);

    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}