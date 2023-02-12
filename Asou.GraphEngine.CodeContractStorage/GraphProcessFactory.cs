﻿using Asou.Abstractions.ExecutionElements;
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
        if (graphProcessContract == null)
        {
            throw new Exception("process is not registered");
        }

        var nodes = graphProcessContract.Nodes.Values.ToDictionary(k => k.Id);
        ElementNode? startNode = null;
        foreach (var connection in graphProcessContract.Graph)
        {
            if (connection.FromElementId == null)
            {
                // for start element
                startNode = nodes[connection.ToElementId];
            }
            else
            {
                CreateNodeConnection(connection.FromElementId.Value, nodes, connection);
            }
        }

        if (startNode == null)
        {
            throw new Exception("Start element does not exists");
        }

        var scope = _serviceProvider.CreateScope();
        var processRuntime = new ProcessRuntime(_parameterDelegateFactory, processContract.Name)
        {
            ComponentFactory = elementType =>
                (BaseElement)scope.ServiceProvider.GetRequiredService(elementType)
        };
        var processInstance = new GraphProcessInstance(processInstanceId, graphProcessContract.ProcessContract,
            processRuntime, startNode, nodes.Values.ToArray(),
            scope);

        return Task.FromResult((IProcessInstance)processInstance);
    }


    private static void CreateNodeConnection(Guid fromId, Dictionary<Guid, ElementNode> nodes,
        ConnectionBuilderInfo connection)
    {
        if (connection.Condition != null)
        {
            nodes[fromId].Connections.Add(new ConditionalConnection
            {
                To = nodes[connection.ToElementId], IsCanNavigate = connection.Condition
            });
        }
        else if (connection.ConditionName != null)
        {
            nodes[fromId].Connections.Add(new ConditionalConnection
            {
                To = nodes[connection.ToElementId],
                IsCanNavigate = (_, from, _) =>
                    ((IPreconfiguredConditions)from).CheckCondition(connection.ConditionName)
            });
        }
        else
        {
            nodes[fromId].Connections.Add(new DefaultConnection { To = nodes[connection.ToElementId] });
        }
    }
}
