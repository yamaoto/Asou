namespace Asou.Abstractions.Container;

/// <summary>Represents a container for a value.</summary>
/// <remarks>
///     This interface is used to represent a container for a value.
/// </remarks>
public interface IContainer
{
    /// <summary>The type of the object.</summary>
    AsouTypes Type { get; init; }

    /// <summary>
    ///     The type of the object, used only when <see cref="AsouTypes" /> is <see cref="AsouTypes.Object" /> or
    ///     <see cref="AsouTypes.ObjectLink" /> .
    /// </summary>
    string ObjectType { get; init; }

    /// <summary>Gets the value from container.</summary>
    /// <returns>The value from container.</returns>
    object? GetValue();
}