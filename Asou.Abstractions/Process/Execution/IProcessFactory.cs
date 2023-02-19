using Asou.Abstractions.Process.Contract;
using Asou.Abstractions.Process.Instance;

namespace Asou.Abstractions.Process.Execution;

public interface IProcessFactory
{
    public Task<IProcessInstance> CreateProcessInstance(Guid processInstanceId, ProcessContract processContract,
        ProcessParameters parameters, CancellationToken cancellationToken = default);
}
