using System.Runtime.CompilerServices;
using Asou.Abstractions.Process.Contract;
using Asou.Abstractions.Process.Execution;
using Asou.Abstractions.Process.Instance;
using Microsoft.Extensions.Logging;

namespace Asou.GraphEngine;

public class GraphProcessExecutionDriver : IProcessExecutionDriver
{
    private readonly ILogger<GraphProcessExecutionDriver> _logger;
    private readonly IProcessFactory _processFactory;

    public GraphProcessExecutionDriver(
        IProcessFactory processFactory,
        ILogger<GraphProcessExecutionDriver> logger)
    {
        _processFactory = processFactory;
        _logger = logger;
    }

    public async Task<IProcessInstance> CreateInstanceAsync(ProcessContract processContract, Guid processInstanceId,
        ProcessParameters parameters, ExecutionOptions executionOptions, CancellationToken cancellationToken = default)
    {
        var processInstance =
            await _processFactory.CreateProcessInstance(processInstanceId, processContract, parameters,
                executionOptions,
                cancellationToken);
        return processInstance;
    }

    public async Task<ProcessParameters?> RunAsync(IProcessInstance processInstance, ExecutionOptions executionOptions,
        CancellationToken cancellationToken = default)
    {
        var instance = Unsafe.As<GraphProcessInstance>(processInstance);
        instance.PrepareStart(executionOptions);
        var result = await ExecuteAsync(instance, executionOptions, cancellationToken);
        return result;
    }

    public async Task ResumeAsync(IProcessInstance processInstance, CancellationToken cancellationToken = default)
    {
        var instance = Unsafe.As<GraphProcessInstance>(processInstance);
        await instance.PrepareResumeAsync(cancellationToken);
        await ExecuteAsync(instance, new ExecutionOptions(true), cancellationToken);
    }

    public async Task<ProcessParameters?> ExecuteAsync(GraphProcessInstance instance, ExecutionOptions executionOptions,
        CancellationToken cancellationToken = default)
    {
        var task = instance.ExecuteAsync(cancellationToken);
        if (executionOptions.RunInBackground)
        {
            void OnCompleted()
            {
                if (task.IsFaulted)
                {
                    _logger.LogError(task.Exception, "Process execution failed. ProcessInstanceId: {ProcessInstanceId}",
                        instance.Id);
                    return;
                }

                _logger.LogDebug("Process execution successfully finished. ProcessInstanceId: {ProcessInstanceId}",
                    instance.Id);
                // TODO: Inform Engine to update process instance state
            }

            var awaiter = task.GetAwaiter();
            if (awaiter.IsCompleted)
            {
                OnCompleted();
            }

            awaiter.OnCompleted(OnCompleted);

            return null;
        }

        try
        {
            await task;
            _logger.LogDebug("Process execution successfully finished. ProcessInstanceId: {ProcessInstanceId}",
                instance.Id);
            return instance.ProcessRuntime.Parameters;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Process execution failed. ProcessInstanceId: {ProcessInstanceId}",
                instance.Id);
            return null;
        }
    }
}
