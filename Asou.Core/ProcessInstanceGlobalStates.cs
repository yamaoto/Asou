namespace Asou.Core;

public static class ProcessInstanceGlobalStates
{
    public const int Created = 1;
    public const int Running = 2;
    public const int Paused = 3;
    public const int Finished = 4;
    public const int Error = 100;
}