using System.Runtime.CompilerServices;
using Asou.Abstractions.Process.Contract;
using Asou.Abstractions.Process.Execution;
using Asou.Abstractions.Process.Instance;

namespace Asou.GraphEngine;

public class GraphProcessExecutionDriver : IProcessExecutionDriver
{
    private readonly IProcessFactory _processFactory;

    public GraphProcessExecutionDriver(
        IProcessFactory processFactory)
    {
        _processFactory = processFactory;
    }

    public async Task<IProcessInstance> CreateInstanceAsync(ProcessContract processContract, Guid processInstanceId,
        ProcessParameters parameters, CancellationToken cancellationToken = default)
    {
        var processInstance =
            await _processFactory.CreateProcessInstance(processInstanceId, processContract, parameters);
        return processInstance;
    }

    public async Task<ProcessParameters?> RunAsync(IProcessInstance processInstance, ExecutionOptions executionOptions,
        CancellationToken cancellationToken = default)
    {
        var instance = Unsafe.As<GraphProcessInstance>(processInstance);
        var task = instance.StartAsync(executionOptions, cancellationToken);
        if (executionOptions.RunInBackground)
        {
            var awaiter = task.GetAwaiter();
            if (awaiter.IsCompleted)
            {
                // TODO: Inform ProcessExecutionEngine about execution stopping
                throw new NotImplementedException();
            }

            awaiter.OnCompleted(() =>
            {
                // TODO: Inform ProcessExecutionEngine about execution stopping
                throw new NotImplementedException();
            });

            return null;
        }

        await task;
        return instance.ProcessRuntime.Parameters;
    }
}
