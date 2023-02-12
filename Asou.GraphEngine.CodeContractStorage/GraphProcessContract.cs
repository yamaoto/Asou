using System.Runtime.CompilerServices;
using Asou.Abstractions.ExecutionElements;
using Asou.Core;

namespace Asou.GraphEngine.CodeContractStorage;

public class GraphProcessContract
{
    internal readonly List<ConnectionBuilderInfo> Graph = new();
    internal readonly Dictionary<string, ElementNode> Nodes = new();

    private string? _currentElement;

    public required ProcessContract ProcessContract { get; init; }

    public static GraphProcessContract Create(Guid processContractId, Guid processVersionId, int versionNumber,
        string name)
    {
        var flow = new GraphProcessContract
        {
            ProcessContract = new ProcessContract(processContractId, processVersionId, versionNumber, name)
        };
        return flow;
    }

    public GraphProcessContract StartFrom<T>(string? to = null, Guid? toId = null) where T : BaseElement
    {
        _currentElement = SetNode<T>(to, toId);
        Graph.Add(new ConnectionBuilderInfo { ToElementId = Nodes[_currentElement].Id });
        return this;
    }

    public GraphProcessContract Then<T>(string? to = null, Guid? toId = null) where T : BaseElement
    {
        if (_currentElement == null)
        {
            throw new Exception("There are not start point");
        }

        var fromVar = _currentElement;
        _currentElement = SetNode<T>(to, toId);
        Graph.Add(new ConnectionBuilderInfo
        {
            FromElementId = Nodes[fromVar].Id, ToElementId = Nodes[_currentElement].Id
        });
        return this;
    }

    public GraphProcessContract Sequence<TFrom, TTo>(string? from = null, string? to = null, Guid? toId = null,
        Guid? fromId = null)
        where TFrom : BaseElement
        where TTo : BaseElement
    {
        var fromVar = SetNode<TFrom>(from, fromId);
        _currentElement = SetNode<TTo>(to, toId);
        Graph.Add(new ConnectionBuilderInfo
        {
            FromElementId = Nodes[fromVar].Id, ToElementId = Nodes[_currentElement].Id
        });
        return this;
    }


    public GraphProcessContract Conditional<TFrom, TTo>(string conditionName,
        string? from = null, string? to = null, Guid? toId = null, Guid? fromId = null)
        where TFrom : BaseElement, IPreconfiguredConditions
        where TTo : BaseElement
    {
        var fromVar = SetNode<TFrom>(from, fromId);
        _currentElement = SetNode<TTo>(to, toId);
        Graph.Add(new ConnectionBuilderInfo
        {
            FromElementId = Nodes[fromVar].Id,
            ToElementId = Nodes[_currentElement].Id,
            ConditionName = conditionName
        });
        return this;
    }

    public GraphProcessContract Conditional<TFrom, TTo>(Func<IProcessInstance, TFrom, TTo, bool> condition,
        string? from = null, string? to = null, Guid? toId = null, Guid? fromId = null)
        where TFrom : BaseElement, IPreconfiguredConditions
        where TTo : BaseElement
    {
        var fromVar = SetNode<TFrom>(from, fromId);
        _currentElement = SetNode<TTo>(to, toId);
        Graph.Add(new ConnectionBuilderInfo
        {
            FromElementId = Nodes[fromVar].Id,
            ToElementId = Nodes[_currentElement].Id,
            Condition = Unsafe.As<IsCanNavigateDelegate>(condition)
        });
        return this;
    }

    public GraphProcessContract WithParameter<TElement, TPropertyType>(string name,
        Func<IProcessInstance, TPropertyType>? getter = null, Action<IProcessInstance, TPropertyType>? setter = null)
        where TElement : BaseElement
    {
        if (_currentElement == null)
        {
            throw new Exception("There are not start point");
        }

        var parameter = new ParameterPersistenceInfo
        {
            Name = name,
            Getter = Unsafe.As<Func<IProcessInstance, object>?>(getter),
            Setter = Unsafe.As<Action<IProcessInstance, object>?>(setter),
            Type = typeof(TPropertyType)
        };
        Nodes[_currentElement].Parameters.Add(parameter);
        return this;
    }

    private string SetNode<T>(string? name = null, Guid? id = null) where T : BaseElement
    {
        var nameVar = name ?? typeof(T).Name;
        var idVar = id ?? Guid.NewGuid();

        if (!Nodes.TryGetValue(nameVar, out var element))
        {
            var type = typeof(T);
            element = new ElementNode
            {
                Id = idVar,
                DisplayName = nameVar,
                ElementType = typeof(T),
                Parameters = new List<ParameterPersistenceInfo>(),
                Connections = new List<IElementNodeConnection>(),
                // TODO: Set IsInclusiveGate properly
                IsInclusiveGate = false,
                UseAsynchronousResume = type.IsAssignableTo(typeof(IAsynchronousResume)),
                UseAfterExecution = type.IsAssignableTo(typeof(IAfterExecution))
            };
            Nodes[nameVar] = element;
        }

        return nameVar;
    }
}
