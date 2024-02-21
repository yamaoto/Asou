using Asou.Abstractions.Process.Instance;

namespace Asou.Abstractions.Process.Execution;

/// <summary>
///     Process instance execution options
/// </summary>
public class ExecutionOptions
{
    /// <summary>
    ///     Process instance execution options
    /// </summary>
    /// <param name="runInBackground">
    ///     When set to true, process instance will be executed in background thread and will not block current thread.
    /// </param>
    /// <param name="executionFlowType">
    ///     Process instance execution flow type determines the process instance is should to run in synchronous or
    ///     asynchronous mode.
    ///     See more: <see cref="Instance.ExecutionFlowType.Asynchronous" /> and <see cref="Instance.ExecutionFlowType.Synchronous" />
    /// </param>
    public ExecutionOptions(bool runInBackground = true,
        ExecutionFlowType executionFlowType = ExecutionFlowType.Asynchronous)
    {
        RunInBackground = runInBackground;
        ExecutionFlowType = executionFlowType;
    }

    /// <summary>
    ///     When set to true, process instance will be executed in background thread and will not block current thread.
    /// </summary>
    public bool RunInBackground { get; init; }

    /// <summary>
    ///     Process instance execution flow type determines the process instance is should to run in synchronous or
    ///     asynchronous mode.
    ///     See more: <see cref="ExecutionFlowType.Asynchronous" /> and <see cref="ExecutionFlowType.Synchronous" />
    /// </summary>
    public ExecutionFlowType ExecutionFlowType { get; init; }
}
