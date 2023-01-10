using System.Diagnostics;

namespace Asou.GraphEngine;

[DebuggerDisplay("ElementNode = {Name}")]
public sealed class ElementNode
{
    public required Type Type { get; init; }
    public required string Name { get; init; }
    public required bool IsShouldAwait { get; init; }
    public required bool IsShouldCallAfterExecute { get; init; }
    public required List<IElementNodeConnection> Connections { get; init; }
    public required bool IsInclusiveGate { get; init; }
    public List<ParameterPersistenceInfo>? Parameters { get; set; }
}