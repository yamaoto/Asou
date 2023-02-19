using Asou.Abstractions.Process.Instance;

namespace Asou.Core.Commands;

public class InsertProcessInstanceStateCommand
{
    private readonly IProcessInstanceRepository _processInstanceRepository;

    public InsertProcessInstanceStateCommand(IProcessInstanceRepository processInstanceRepository)
    {
        _processInstanceRepository = processInstanceRepository;
    }

    public async Task ActivateAsync(IProcessInstance processInstance, ProcessInstanceState state,
        CancellationToken cancellationToken = default)
    {
        await _processInstanceRepository.CreateInstanceAsync(processInstance.Id,
            processInstance.ProcessContract.ProcessContractId,
            processInstance.ProcessContract.ProcessVersionId,
            processInstance.ProcessContract.VersionNumber, processInstance.PersistenceType,
            state, cancellationToken);
    }
}
