using Asou.Abstractions;

namespace Asou.ByteCodeEngine;

public class StorageContainer
{
    public AsouTypes Type { get; set; }
    public required string ObjectType { get; init; }
    public string? Parameter { get; init; }
    public string? Value { get; init; }
}