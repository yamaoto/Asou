using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using Asou.Abstractions;
using Asou.Abstractions.Container;

namespace Asou.ByteCodeEngine;

/// <summary>Reads a byte code format.</summary>
/// <param name="code">The byte code.</param>
/// <returns>The byte code format.</returns>
public sealed class ByteCodeFormatReader
{
    private readonly byte[] _code;

    public ByteCodeFormatReader(byte[] code)
    {
        _code = code;
    }

    public bool IsEndOfCode => Position >= _code.Length;

    public long Position { get; private set; }

    /// <summary>Seeks to the specified offset.</summary>
    /// <param name="offset">The offset.</param>
    /// <param name="origin">The origin.</param>
    /// <exception cref="InvalidOperationException">Thrown when the origin is not supported.</exception>
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

    /// <summary>Reads a string from the byte code.</summary>
    /// <returns>The string read from the byte code.</returns>
    public string ReadString()
    {
        var initial = Convert.ToInt32(Position);
        while (_code[Position] != 0) Position++;

        if (initial == Position) return "";

        var result = Encoding.UTF8.GetString(_code, initial, Convert.ToInt32(Position) - initial);

        Position++;

        return result;
    }

    /// <summary>Reads a byte from the byte code.</summary>
    /// <returns>The byte byte read.</returns>
    public byte ReadByte()
    {
        var result = _code[Position];
        Position++;
        return result;
    }
    
    /// <summary>Reads a sequence of bytes from the byte code.</summary>
    /// <param name="count">The number of bytes to read.</param>
    /// <returns>The bytes read from the current stream.</returns>
    public ReadOnlySpan<T> ReadBytes<T>(int count)
    {
        var result = new T[count];
        Array.Copy(_code, Position, result, 0, count);
        Position += count;
        return new ReadOnlySpan<T>(result);
    }

    /// <summary>Reads a type from byte code.</summary>       
    /// <returns>type.</returns>       
    public AsouTypes ReadType()
    {
        return (AsouTypes)ReadByte();
    }

    /// <summary>Reads a boolean value from the byte code.</summary>
    /// <returns>The boolean value that was read.</returns>
    public bool ReadBoolean()
    {
        return ReadByte() != 0;
    }

    /// <summary>Reads an Int32 from the byte code.</summary>
    /// <returns>The Int32 that was read.</returns>
    public int ReadInt32()
    {
        return BinaryPrimitives.ReadInt32LittleEndian(ReadBytes<byte>(4));
    }

    /// <summary>Reads a single-precision floating point number from the byte code.</summary>
    /// <returns>The single-precision floating point number that was read.</returns>
    public float ReadSingle()
    {
        return BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(ReadBytes<byte>(4)));
    }

    /// <summary>Reads a decimal value from the byte code.</summary>
    /// <returns>The decimal value read from the byte code.</returns>
    public decimal ReadDecimal()
    {
        return new(ReadBytes<int>(16));
    }

    /// <summary>Reads a value from the byte code.</summary>
    /// <param name="type">The type of the value to read.</param>
    /// <returns>The value read from the byte code.</returns>
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

    /// <summary>Reads a valur from value container from the byte code.</summary>
    /// <returns>The value from value container readed from the byte code.</returns>
    public object? ReadContainerValue()
    {
        var container = ReadContainer();
        throw new NotImplementedException();
    }

    /// <summary>Reads a value container from the byte code.</summary>
    /// <returns>The value container readed from the byte code.</returns>
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