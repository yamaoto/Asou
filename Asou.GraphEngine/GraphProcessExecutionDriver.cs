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
        CancellationToken cancellationToken = default)
    {
        var processInstance = await _processFactory.CreateProcessInstance(processInstanceId, processContract);
        return processInstance;
    }

    public async Task<ProcessParameters> RunAsync(IProcessInstance processInstance,
        CancellationToken cancellationToken = default)
    {
        var instance = Unsafe.As<GraphProcessInstance>(processInstance);
        await instance.StartAsync(cancellationToken);
        return instance.ProcessRuntime.Parameters;
    }
}
