namespace Asou.GraphEngine;

public static class ExecutionStatuses
{
    public const int Break = -2;
    public const int Exit = -1;

    public const int Execute = 0;

    public const int AfterExecute = 1;

    public const int ConfigureAwaiter = 2;
}