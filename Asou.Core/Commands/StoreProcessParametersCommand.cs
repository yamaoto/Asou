using Asou.Abstractions.Process.Execution;

namespace Asou.Core.Commands;

public class StoreProcessParametersCommand
{
    private readonly IProcessExecutionPersistenceRepository _processExecutionPersistenceRepository;

    public StoreProcessParametersCommand(IProcessExecutionPersistenceRepository processExecutionPersistenceRepository)
    {
        _processExecutionPersistenceRepository = processExecutionPersistenceRepository;
    }

    public async Task ActivateAsync(StoreProcessParameters[] payload, CancellationToken cancellationToken = default)
    {
        foreach (var (instanceId, parameters) in payload)
        {
            await _processExecutionPersistenceRepository.StoreProcessParameterAsync(instanceId, parameters,
                cancellationToken);
        }
    }
}
