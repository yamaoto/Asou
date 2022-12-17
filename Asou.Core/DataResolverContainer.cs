namespace Asou.Core;

public class DataResolverContainer : IContainer
{
    public required string Parameter { get; init; }
    public AsouTypes Type { get; init; }
    public required string ObjectType { get; init; }

    public object? GetValue()
    {
        return null;
    }
}