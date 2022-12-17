namespace Asou.Core;

public class ByteCodeStorage
{
    private readonly Dictionary<string, byte[]> _processCodes = new();

    public void Register(string processName, byte[] code)
    {
        _processCodes[processName] = code;
    }

    public byte[] GetCodeForProcess(string processName)
    {
        return _processCodes[processName];
    }
}