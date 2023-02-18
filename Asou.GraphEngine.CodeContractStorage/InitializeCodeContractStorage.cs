using Asou.Abstractions;
using Microsoft.Extensions.Logging;

namespace Asou.GraphEngine.CodeContractStorage;

public class InitializeCodeContractStorage : IInitializeHook
{
    private readonly IGraphProcessRegistration _graphProcessRegistration;
    private readonly IEnumerable<IProcessDefinition> _processDefinitions;
    private readonly ILoggerFactory _loggerFactory;

    public InitializeCodeContractStorage(
        IGraphProcessRegistration graphProcessRegistration,
        IEnumerable<IProcessDefinition> processDefinitions,
        ILoggerFactory loggerFactory)
    {
        _graphProcessRegistration = graphProcessRegistration;
        _processDefinitions = processDefinitions;
        _loggerFactory = loggerFactory;
    }

    public Task Initialize(CancellationToken cancellationToken = default)
    {
        foreach (var processDefinition in _processDefinitions)
        {
            var builder = GraphProcessContract.Create(processDefinition.Id, processDefinition.VersionId,
                processDefinition.Version, processDefinition.Name, _loggerFactory.CreateLogger<GraphProcessContract>());
            processDefinition.Describe(builder);
            _graphProcessRegistration.RegisterFlow(builder);
        }

        return Task.CompletedTask;
    }
}
