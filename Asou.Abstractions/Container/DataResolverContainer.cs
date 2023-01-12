namespace Asou.Abstractions.Container;

/// <summary>A container for a data resolver.</summary>
/// <remarks>
/// This class is used to pass data resolver information to the <see cref="DataResolver"/> class.
/// </remarks>
public class DataResolverContainer : IContainer
{
    public required string Parameter { get; init; }

    /// <summary>The type of the object.</summary>
    public AsouTypes Type { get; init; }

    /// <summary>The type of the object, used only when <see cref="AsouTypes" /> is <see cref="AsouTypes.Object" /> or  <see cref="AsouTypes.ObjectLink" /> .</summary>
    public required string ObjectType { get; init; }

    /// <summary>Gets the value from container.</summary>
    /// <returns>The value from container.</returns>
    public object? GetValue()
    {
        return null;
    }
}