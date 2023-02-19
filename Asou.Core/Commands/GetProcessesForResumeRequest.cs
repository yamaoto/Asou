using System.Runtime.CompilerServices;
using Asou.Abstractions.Process.Contract;
using Asou.Abstractions.Process.Execution;
using Asou.Abstractions.Process.Instance;

namespace Asou.Core.Commands;

public class GetProcessesForResumeRequest
{
    private readonly IProcessContractRepository _processContractRepository;
    private readonly IProcessExecutionPersistenceRepository _processExecutionPersistenceRepository;
    private readonly IProcessInstanceRepository _processInstanceRepository;


    public GetProcessesForResumeRequest(IProcessInstanceRepository processInstanceRepository,
        IProcessContractRepository processContractRepository,
        IProcessExecutionPersistenceRepository processExecutionPersistenceRepository)
    {
        _processInstanceRepository = processInstanceRepository;
        _processContractRepository = processContractRepository;
        _processExecutionPersistenceRepository = processExecutionPersistenceRepository;
    }

    public Task ActivateAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public async IAsyncEnumerable<ResumeProcessRecord> GetAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var instances = await _processInstanceRepository.GetRunningInstancesAsync(cancellationToken);
        foreach (var instance in instances)
        {
            var processContract = await _processContractRepository.GetProcessContractAsync(instance.ProcessContractId,
                instance.ProcessVersionId, instance.Version);
            var parameters =
                await _processExecutionPersistenceRepository.GetProcessParametersAsync(instance.Id, cancellationToken);
            yield return new ResumeProcessRecord(instance, processContract, parameters);
        }
    }
}
