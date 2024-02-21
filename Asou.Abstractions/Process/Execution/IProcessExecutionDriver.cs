using Asou.Abstractions.Process.Contract;
using Asou.Abstractions.Process.Instance;

namespace Asou.Abstractions.Process.Execution;

public interface IProcessExecutionDriver
{
    Task<IProcessInstance> CreateInstanceAsync(ProcessContract processContract, Guid processInstanceId,
        ProcessParameters parameters, ExecutionOptions executionOptions, CancellationToken cancellationToken = default);

    Task<ProcessParameters?> RunAsync(IProcessInstance processInstance, ExecutionOptions executionOptions,
        CancellationToken cancellationToken = default);

    Task ResumeAsync(IProcessInstance processInstance, CancellationToken cancellationToken = default);
}
