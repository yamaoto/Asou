namespace Asou.Abstractions.Container;

public interface IContainer
{
    AsouTypes Type { get; init; }
    string ObjectType { get; init; }

    object? GetValue();
}