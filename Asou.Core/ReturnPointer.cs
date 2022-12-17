namespace Asou.Core;

public struct ReturnPointer
{
    public long Position;
    public long ReadUntil;

    public ReturnPointer(long position, long readUntil)
    {
        Position = position;
        ReadUntil = readUntil;
    }
}