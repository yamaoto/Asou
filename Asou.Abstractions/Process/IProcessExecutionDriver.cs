namespace Asou.Abstractions.Process;

public interface IProcessExecutionDriver
{
    Task<IProcessInstance> CreateInstanceAsync(ProcessContract processContract,
        CancellationToken cancellationToken = default);

    Task<ProcessParameters> RunAsync(IProcessInstance processInstance,
        CancellationToken cancellationToken = default);
}
