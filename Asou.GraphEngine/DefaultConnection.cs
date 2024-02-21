namespace Asou.GraphEngine;

public class DefaultConnection : IGraphElementConnection
{
    public required GraphElement To { get; init; }
}
