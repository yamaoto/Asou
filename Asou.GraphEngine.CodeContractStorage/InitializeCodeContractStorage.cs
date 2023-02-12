using Asou.Abstractions;

namespace Asou.GraphEngine.CodeContractStorage;

public class InitializeCodeContractStorage : IInitializeHook
{
    private readonly IGraphProcessRegistration _graphProcessRegistration;
    private readonly IEnumerable<IProcessDefinition> _processDefinitions;

    public InitializeCodeContractStorage(
        IGraphProcessRegistration graphProcessRegistration,
        IEnumerable<IProcessDefinition> processDefinitions)
    {
        _graphProcessRegistration = graphProcessRegistration;
        _processDefinitions = processDefinitions;
    }

    public Task Initialize(CancellationToken cancellationToken = default)
    {
        foreach (var processDefinition in _processDefinitions)
        {
            var builder = GraphProcessContract.Create(processDefinition.Id, processDefinition.VersionId,
                processDefinition.Version, processDefinition.Name);
            processDefinition.Describe(builder);
            _graphProcessRegistration.RegisterFlow(builder);
        }

        return Task.CompletedTask;
    }
}
