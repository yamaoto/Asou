using System.Diagnostics;

namespace Asou.Abstractions.ExecutionElements;

[DebuggerDisplay("{ClassName}")]
public abstract class BaseElement
{
    public abstract string ClassName { get; init; }
    public abstract Task ExecuteAsync(CancellationToken cancellationToken = default);
}