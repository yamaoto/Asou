using Asou.Core.Abstractions;

namespace Asou.Core.Container;

public interface IContainer
{
    AsouTypes Type { get; init; }
    string ObjectType { get; init; }

    object? GetValue();
}