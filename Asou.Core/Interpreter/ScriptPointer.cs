namespace Asou.Core.Interpreter;

public struct ScriptPointer
{
    public long Start;
    public int Length;

    public ScriptPointer(long start, int length)
    {
        Start = start;
        Length = length;
    }
}