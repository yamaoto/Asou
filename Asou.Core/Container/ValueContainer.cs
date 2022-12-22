using System.Text.Json;
using Asou.Abstractions;

namespace Asou.Core.Container;

public class ValueContainer : IContainer
{
    public required string Value { get; init; }
    public AsouTypes Type { get; init; }
    public required string ObjectType { get; init; }

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
                return JsonSerializer.Deserialize(Value, System.Type.GetType(ObjectType)!);
            case AsouTypes.ObjectLink:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}