using Asou.Abstractions.ExecutionElements;
using Asou.Core;
using Asou.Core.Process;
using Asou.Core.Process.Binding;
using Microsoft.Extensions.DependencyInjection;

namespace Asou.GraphEngine.CodeContractStorage;

public class GraphProcessFactory : IGraphProcessFactory
{
    private readonly IGraphProcessContractRepository _graphProcessContractRepository;
    private readonly IParameterDelegateFactory _parameterDelegateFactory;
    private readonly IServiceProvider _serviceProvider;

    public GraphProcessFactory(
        IGraphProcessContractRepository graphProcessContractRepository,
        IServiceProvider serviceProvider,
        IParameterDelegateFactory parameterDelegateFactory
    )
    {
        _graphProcessContractRepository = graphProcessContractRepository;
        _serviceProvider = serviceProvider;
        _parameterDelegateFactory = parameterDelegateFactory;
    }

    public Task<IProcessInstance> CreateProcessInstance(Guid processInstanceId, ProcessContract processContract)
    {
        var graphProcessContract = _graphProcessContractRepository.GetGraphProcessContract(
            processContract.ProcessContractId,
            processContract.ProcessVersionId, processContract.VersionNumber);
        if (graphProcessContract == null) throw new Exception("process is not registered");

        var nodes = new Dictionary<string, ElementNode>();
        ElementNode? startNode = null;
        foreach (var connection in graphProcessContract.Graph)
            if (connection.From == null)
                // for start element
                startNode = CreateNode(connection.To, nodes, connection.ToType);
            else
                CreateNodeConnection(connection.From, nodes, connection);

        // TODO: rearrange node creation. first create nodes, then create connections 
        foreach (var (name, node) in graphProcessContract.Nodes)
            if (nodes.ContainsKey(name))
                nodes[name].Parameters = node.Parameters;

        if (startNode == null) throw new Exception("Start element does not exists");

        var scope = _serviceProvider.CreateScope();
        var processRuntime = new ProcessRuntime(_parameterDelegateFactory, processContract.Name)
        {
            ComponentFactory = (componentName, objectName) =>
                (BaseElement)scope.ServiceProvider.GetRequiredService(nodes[componentName].Type)
        };
        var processInstance = new GraphProcessInstance(processInstanceId, graphProcessContract.ProcessContract,
            processRuntime, startNode, nodes.Values.ToList(),
            scope);

        return Task.FromResult((IProcessInstance)processInstance);
    }


    private ElementNode CreateNode(string name, Dictionary<string, ElementNode> nodes, Type type)
    {
        return nodes[name] = new ElementNode
        {
            Name = name,
            Connections = new List<IElementNodeConnection>(),
            IsInclusiveGate = true,
            IsShouldAwait = type.IsAssignableTo(typeof(IAsyncExecutionElement)),
            IsShouldCallAfterExecute = type.IsAssignableTo(typeof(IAfterExecute)),
            Type = type
        };
    }

    private void CreateNodeConnection(string fromName, Dictionary<string, ElementNode> nodes,
        ConnectionBuilderInfo connection)
    {
        if (!nodes.ContainsKey(fromName)) CreateNode(fromName, nodes, connection.FromType!);

        if (!nodes.ContainsKey(connection.To)) CreateNode(connection.To, nodes, connection.ToType);

        if (connection.Condition != null)
            nodes[fromName].Connections.Add(new ConditionalConnection
            {
                To = nodes[connection.To], IsCanNavigate = connection.Condition
            });
        else if (connection.ConditionName != null)
            nodes[fromName].Connections.Add(new ConditionalConnection
            {
                To = nodes[connection.To],
                IsCanNavigate = (_, from, _) =>
                    ((IPreconfiguredConditions)from).CheckCondition(connection.ConditionName)
            });
        else
            nodes[fromName].Connections.Add(new DefaultConnection { To = nodes[connection.To] });
    }
}