using System.Text;
using Asou.Abstractions;

namespace Asou.ByteCodeEngine;

public class ByteCodeWriter : BinaryWriter
{
    public ByteCodeWriter(Stream stream) : base(stream, Encoding.UTF8)
    {
    }

    public override void Write(string value)
    {
        Write(Encoding.UTF8.GetBytes(value));
        Write((byte)0);
    }

    public void DeclareScript(string name, Action<ByteCodeWriter> action)
    {
        var stream = new MemoryStream();
        var inner = new ByteCodeWriter(stream);
        action(inner);
        inner.Write((byte)Instructions.Return);

        Write((byte)Instructions.DeclareScript);
        Write(name);
        var length = Convert.ToInt32(stream.Position);
        Write(length);
        var position = stream.Position;
        stream.Seek(0, 0);
        var buffer = new byte[position];
        stream.Read(buffer, 0, length);
        Write(buffer);
    }

    public void CreateComponent(string componentName, string name)
    {
        Write((byte)Instructions.CreateComponent);
        Write(componentName);
        Write(name);
    }

    public void ExecuteElement(string name)
    {
        Write((byte)Instructions.ExecuteElement);
        Write(name);
    }

    public void CallScript(string name)
    {
        Write((byte)Instructions.CallScript);
        Write(name);
    }

    public void LetParameter(string name)
    {
        Write((byte)Instructions.LetParameter);
        Write(name);
    }

    public void SetParameter(string name, AsouTypes type, Action<ByteCodeWriter> action)
    {
        Write((byte)Instructions.SetParameter);
        Write(name);
        Write((byte)type);
        action(this);
    }
}
