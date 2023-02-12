using System.Diagnostics;

namespace Asou.GraphEngine;

[DebuggerDisplay("ElementNode = {DisplayName}")]
public sealed class ElementNode
{
    public Guid Id { get; init; }
    public required Type ElementType { get; init; }
    public required string DisplayName { get; init; }
    public required List<IElementNodeConnection> Connections { get; init; }
    public required List<ParameterPersistenceInfo> Parameters { get; init; }

    public required bool IsInclusiveGate { get; init; }
    public required bool UseAsynchronousResume { get; init; }
    public required bool UseAfterExecution { get; init; }
}
