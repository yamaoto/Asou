namespace Asou.Abstractions.Process.Instance;

public enum ProcessInstanceState : byte
{
    No = 0,
    New = 1,
    Running = 2,
    Error = 3,
    Finished = 4
}
