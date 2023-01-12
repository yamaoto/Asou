using System.Diagnostics;

namespace Asou.Abstractions.ExecutionElements;

/// <summary>Base class for all elements.</summary>
[DebuggerDisplay("{ClassName}")]
public abstract class BaseElement
{
    /// <summary>Gets the name of the class.</summary>
    /// <returns>The name of the class.</returns>
    [Obsolete]
    public abstract string ClassName { get; init; }

    /// <summary>Executes the command.</summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the operation.</returns>
    public abstract Task ExecuteAsync(CancellationToken cancellationToken = default);
}