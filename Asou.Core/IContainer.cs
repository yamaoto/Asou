namespace Asou.Core;

public interface IContainer
{
    AsouTypes Type { get; init; }
    string ObjectType { get; init; }

    object? GetValue();
}