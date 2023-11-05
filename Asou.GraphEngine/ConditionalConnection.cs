namespace Asou.GraphEngine;

public class ConditionalConnection : IGraphElementConnection
{
    public required IsCanNavigateDelegate IsCanNavigate { get; init; }
    public required GraphElement To { get; init; }
}
