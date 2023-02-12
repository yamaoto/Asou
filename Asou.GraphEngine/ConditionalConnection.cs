namespace Asou.GraphEngine;

public class ConditionalConnection : IElementNodeConnection
{
    public required IsCanNavigateDelegate IsCanNavigate { get; init; }
    public required ElementNode To { get; init; }
}
