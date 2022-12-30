using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using Asou.Abstractions;
using Asou.Core.Container;

namespace Asou.Core.Interpreter;

public sealed class ByteCodeFormatReader
{
    private readonly byte[] _code;

    public ByteCodeFormatReader(byte[] code)
    {
        _code = code;
    }

    public bool IsEndOfCode => Position >= _code.Length;

    public long Position { get; private set; }

    public void Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                Position = offset;
                break;
            case SeekOrigin.Current:
                Position += offset;
                break;
            case SeekOrigin.End:
                throw new InvalidOperationException();
            default:
                throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
        }
    }

    public string ReadString()
    {
        var initial = Convert.ToInt32(Position);
        while (_code[Position] != 0) Position++;

        if (initial == Position)
            return "";

        var result = Encoding.UTF8.GetString(_code, initial, Convert.ToInt32(Position) - initial);

        Position++;

        return result;
    }

    public byte ReadByte()
    {
        var result = _code[Position];
        Position++;
        return result;
    }

    public ReadOnlySpan<T> ReadBytes<T>(int count)
    {
        var result = new T[count];
        Array.Copy(_code, Position, result, 0, count);
        Position += count;
        return new ReadOnlySpan<T>(result);
    }

    public AsouTypes ReadType()
    {
        return (AsouTypes)ReadByte();
    }

    public bool ReadBoolean()
    {
        return ReadByte() != 0;
    }

    public int ReadInt32()
    {
        return BinaryPrimitives.ReadInt32LittleEndian(ReadBytes<byte>(4));
    }

    public float ReadSingle()
    {
        return BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(ReadBytes<byte>(4)));
    }

    public decimal ReadDecimal()
    {
        return new decimal(ReadBytes<int>(16));
    }

    public object? ReadValue(AsouTypes type)
    {
        switch (type)
        {
            case AsouTypes.NullObject:
            case AsouTypes.UnSet:
                return null;
            case AsouTypes.Boolean:
                return ReadBoolean();
            case AsouTypes.Integer:
                return ReadInt32();
            case AsouTypes.Float:
                return ReadSingle();
            case AsouTypes.Decimal:
                return ReadDecimal();
            case AsouTypes.String:
                return ReadString();
            case AsouTypes.DateTime:
                return JsonSerializer.Deserialize<DateTime>(ReadString());
            case AsouTypes.Guid:
                return JsonSerializer.Deserialize<Guid>(ReadString());
            case AsouTypes.Object:
                return ReadContainerValue();
            case AsouTypes.ObjectLink:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public object? ReadContainerValue()
    {
        var container = ReadContainer();
        throw new NotImplementedException();
    }

    public IContainer ReadContainer()
    {
        var storageContainer = JsonSerializer.Deserialize<StorageContainer>(ReadString())!;
        if (string.IsNullOrEmpty(storageContainer.Parameter))
            return new ValueContainer
            {
                Type = storageContainer.Type,
                ObjectType = storageContainer.ObjectType,
                Value = storageContainer.Value!
            };

        return new DataResolverContainer
        {
            Type = storageContainer.Type,
            ObjectType = storageContainer.ObjectType,
            Parameter = storageContainer.Parameter
        };
    }
}