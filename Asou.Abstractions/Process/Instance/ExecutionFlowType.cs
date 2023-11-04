namespace Asou.Abstractions.Process.Instance;

/// <summary>
///     Process instance execution flow type determines the process instance is should to run in synchronous or
///     asynchronous mode.
/// </summary>
public enum ExecutionFlowType : byte
{
    /// <summary>
    ///     This is default scenario. Process instance will be executed asynchronously. This means when process gets
    ///     asynchronous resume and there is no active thread, process state will be saved and process instance will be
    ///     unloaded from memory and will be loaded again when new event will be received.
    /// </summary>
    Asynchronous = 0,

    /// <summary>
    ///     Please read <see cref="Asynchronous" /> description. This is opposite scenario with additional feature to allow
    ///     running process in C# code just like regular async/await method and wait for process instance to finish with
    ///     <see cref="System.Threading.Tasks.Task" />
    ///     Example:
    ///     var result = await processExecutionEngine.CreateAndExecuteAsync(processId, processParameters, , new
    ///     ExecutionOptions(false), cancellationToken);
    /// </summary>
    Synchronous = 1
}
