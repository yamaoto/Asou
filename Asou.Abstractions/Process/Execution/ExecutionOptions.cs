namespace Asou.Abstractions.Process.Execution;

public class ExecutionOptions
{
    public ExecutionOptions()
    {
    }

    public ExecutionOptions(bool runInBackground)
    {
        RunInBackground = runInBackground;
    }

    public bool RunInBackground { get; init; }
}
