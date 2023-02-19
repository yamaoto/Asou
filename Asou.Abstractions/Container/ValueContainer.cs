using System.Text.Json;

namespace Asou.Abstractions.Container;

/// <summary>A container for a value.</summary>
/// <remarks>
///     This class is used to store the value of a property in an object.
/// </remarks>
public sealed class ValueContainer : IContainer
{
    /// <summary>Serialized string of container stored in JSON format.</summary>
    public required string Value { get; init; }

    /// <summary>The type of the object.</summary>
    public AsouTypes Type { get; init; }

    /// <summary>
    ///     The type of the object, used only when <see cref="AsouTypes" /> is <see cref="AsouTypes.Object" /> or
    ///     <see cref="AsouTypes.ObjectLink" /> .
    /// </summary>
    public required string ObjectType { get; init; }

    /// <summary>Gets the value from container.</summary>
    /// <returns>The value from container.</returns>
    public object? GetValue()
    {
        switch (Type)
        {
            case AsouTypes.UnSet:
            case AsouTypes.NullObject:
                return null;
            case AsouTypes.Boolean:
                return JsonSerializer.Deserialize<bool>(Value);
            case AsouTypes.Integer:
                return JsonSerializer.Deserialize<int>(Value);
            case AsouTypes.Float:
                return JsonSerializer.Deserialize<float>(Value);
            case AsouTypes.Decimal:
                return JsonSerializer.Deserialize<decimal>(Value);
            case AsouTypes.String:
                return JsonSerializer.Deserialize<string>(Value);
            case AsouTypes.DateTime:
                return JsonSerializer.Deserialize<DateTime>(Value);
            case AsouTypes.Guid:
                return JsonSerializer.Deserialize<Guid>(Value);
            case AsouTypes.Object:
                return DeserializeObject();
            case AsouTypes.ObjectLink:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private object? DeserializeObject()
    {
        if (string.IsNullOrEmpty(ObjectType))
        {
            return null;
        }

        return JsonSerializer.Deserialize(Value, System.Type.GetType(ObjectType)!);
    }
}
