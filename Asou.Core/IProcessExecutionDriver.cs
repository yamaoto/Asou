using Asou.Abstractions;

namespace Asou.Core;

public interface IProcessExecutionDriver
{
    Task InitializeAsync();

    Task<IProcessInstance> CreateInstanceAsync(ProcessContract processContract,
        CancellationToken cancellationToken = default);

    Task<ProcessParameters> RunAsync(IProcessInstance processInstance,
        CancellationToken cancellationToken = default);
}