using System.Runtime.CompilerServices;
using Asou.Abstractions;
using Asou.Core;

namespace Asou.GraphEngine;

public class GraphProcessExecutionDriver : IProcessExecutionDriver
{
    private readonly IProcessFactory _processFactory;

    public GraphProcessExecutionDriver(
        IProcessFactory processFactory)
    {
        _processFactory = processFactory;
    }

    public Task InitializeAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IProcessInstance> CreateInstanceAsync(ProcessContract processContract,
        CancellationToken cancellationToken = default)
    {
        var processInstanceId = Guid.NewGuid();
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