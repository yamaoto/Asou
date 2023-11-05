using Asou.Abstractions;
using Asou.Abstractions.ExecutionElements;
using Asou.Abstractions.Process.Contract;
using Asou.Abstractions.Process.Execution;
using Asou.Abstractions.Process.Instance;
using Asou.Core.Process;
using Asou.Core.Process.Binding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Asou.GraphEngine.CodeContractStorage;

public class GraphProcessFactory : IGraphProcessFactory
{
    private readonly IGraphProcessContractRepository _graphProcessContractRepository;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IParameterDelegateFactory _parameterDelegateFactory;
    private readonly IServiceProvider _serviceProvider;

    public GraphProcessFactory(
        IGraphProcessContractRepository graphProcessContractRepository,
        IServiceProvider serviceProvider,
        IParameterDelegateFactory parameterDelegateFactory,
        ILoggerFactory loggerFactory
    )
    {
        _graphProcessContractRepository = graphProcessContractRepository;
        _serviceProvider = serviceProvider;
        _parameterDelegateFactory = parameterDelegateFactory;
        _loggerFactory = loggerFactory;
    }

    public Task<IProcessInstance> CreateProcessInstance(Guid processInstanceId, ProcessContract processContract,
        ProcessParameters parameters, ExecutionOptions executionOptions, CancellationToken cancellationToken = default)
    {
        var graphProcessContract = _graphProcessContractRepository.GetGraphProcessContract(
            processContract.ProcessContractId,
            processContract.ProcessVersionId, processContract.VersionNumber);
        if (graphProcessContract == null)
        {
            throw new Exception("process is not registered");
        }

        var elements = graphProcessContract.Elements.Values.ToDictionary(k => k.Id);
        GraphElement? startElement = null;
        foreach (var connection in graphProcessContract.Graph)
        {
            if (connection.FromElementId == null)
            {
                // for start element
                startElement = elements[connection.ToElementId];
            }
            else
            {
                CreateElementConnection(connection.FromElementId.Value, elements, connection);
            }
        }

        if (startElement == null)
        {
            throw new Exception("Start element does not exists");
        }

        var scope = _serviceProvider.CreateScope();
        var processRuntime = new ProcessRuntime(_parameterDelegateFactory, processContract.Name)
        {
            ComponentFactory = elementType =>
                (BaseElement)scope.ServiceProvider.GetRequiredService(elementType)
        };
        var logger = _loggerFactory.CreateLogger<GraphProcessInstance>();
        var processInstance = new GraphProcessInstance(processInstanceId, graphProcessContract.ProcessContract,
            processRuntime, startElement, elements.Values.ToArray(), graphProcessContract.PersistenceType,
            executionOptions.ExecutionFlowType, scope, logger);

        foreach (var (parameter, value) in parameters)
        {
            processInstance.ProcessRuntime.SetParameter(parameter, AsouTypes.UnSet, value);
        }

        return Task.FromResult((IProcessInstance)processInstance);
    }


    private static void CreateElementConnection(Guid fromId, Dictionary<Guid, GraphElement> elements,
        ConnectionBuilderInfo connection)
    {
        if (connection.Condition != null)
        {
            elements[fromId].Connections.Add(new ConditionalConnection
            {
                To = elements[connection.ToElementId], IsCanNavigate = connection.Condition
            });
        }
        else if (connection.ConditionName != null)
        {
            elements[fromId].Connections.Add(new ConditionalConnection
            {
                To = elements[connection.ToElementId],
                IsCanNavigate = (_, from, _) =>
                    ((IPreconfiguredConditions)from).CheckCondition(connection.ConditionName)
            });
        }
        else
        {
            elements[fromId].Connections.Add(new DefaultConnection { To = elements[connection.ToElementId] });
        }
    }
}
