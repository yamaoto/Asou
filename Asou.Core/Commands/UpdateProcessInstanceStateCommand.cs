using Asou.Abstractions.Process.Instance;

namespace Asou.Core.Commands;

public class UpdateProcessInstanceStateCommand
{
    private readonly IProcessInstanceRepository _processInstanceRepository;

    public UpdateProcessInstanceStateCommand(IProcessInstanceRepository processInstanceRepository)
    {
        _processInstanceRepository = processInstanceRepository;
    }

    public async Task ActivateAsync(Guid processInstanceId, ProcessInstanceState state,
        CancellationToken cancellationToken = default)
    {
        await _processInstanceRepository.UpdateStateInstanceAsync(processInstanceId, state,
            cancellationToken);
    }
}
