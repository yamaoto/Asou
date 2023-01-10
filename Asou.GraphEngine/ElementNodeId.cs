using System.Diagnostics;

namespace Asou.GraphEngine;

[DebuggerDisplay("ElementNode = {Node.Name} ({Id})")]
internal sealed class ElementNodeId
{
    public ElementNodeId(Guid id, ElementNode node)
    {
        Id = id;
        Node = node;
    }

    public Guid Id { get; }
    public ElementNode Node { get; }
}