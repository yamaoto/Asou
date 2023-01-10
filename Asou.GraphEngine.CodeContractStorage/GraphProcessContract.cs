using System.Runtime.CompilerServices;
using Asou.Abstractions.ExecutionElements;
using Asou.Core;

namespace Asou.GraphEngine.CodeContractStorage;

public class GraphProcessContract
{
    internal readonly List<ConnectionBuilderInfo> Graph = new();
    internal readonly Dictionary<string, NodeBuilderInfo> Nodes = new();

    private ConnectionBuilderInfo? _current;

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

    public GraphProcessContract StartFrom<T>(string? to = null) where T : BaseElement
    {
        var item = new ConnectionBuilderInfo { To = to ?? typeof(T).Name, ToType = typeof(T) };
        Graph.Add(item);
        _current = item;
        return this;
    }

    public GraphProcessContract Then<T>(string? to = null) where T : BaseElement
    {
        if (_current == null) throw new Exception("There are not start point");

        var item = new ConnectionBuilderInfo
        {
            From = _current.To, FromType = _current.ToType, To = to ?? typeof(T).Name, ToType = typeof(T)
        };
        Graph.Add(item);
        _current = item;
        return this;
    }

    public GraphProcessContract Sequence<TFrom, TTo>(string? from = null, string? to = null)
        where TTo : BaseElement
    {
        var item = new ConnectionBuilderInfo
        {
            From = from ?? typeof(TFrom).Name,
            FromType = typeof(TFrom),
            To = to ?? typeof(TTo).Name,
            ToType = typeof(TTo)
        };
        Graph.Add(item);
        _current = item;
        return this;
    }

    public GraphProcessContract Conditional<TFrom, TTo>(string conditionName, string? from = null, string? to = null)
        where TFrom : BaseElement, IPreconfiguredConditions
        where TTo : BaseElement
    {
        var item = new ConnectionBuilderInfo
        {
            From = from ?? typeof(TFrom).Name,
            FromType = typeof(TFrom),
            To = to ?? typeof(TTo).Name,
            ToType = typeof(TTo),
            ConditionName = conditionName
        };
        Graph.Add(item);
        _current = item;
        return this;
    }

    public GraphProcessContract Conditional<TFrom, TTo>(Func<IProcessInstance, TFrom, TTo, bool> condition,
        string? from = null, string? to = null)
        where TFrom : BaseElement, IPreconfiguredConditions
        where TTo : BaseElement
    {
        var item = new ConnectionBuilderInfo
        {
            From = from ?? typeof(TFrom).Name,
            FromType = typeof(TFrom),
            To = to ?? typeof(TTo).Name,
            ToType = typeof(TTo),
            Condition = Unsafe.As<IsCanNavigateDelegate>(condition)
        };
        Graph.Add(item);
        _current = item;
        return this;
    }

    public GraphProcessContract WithParameter<TElement, TPropertyType>(string name,
        Func<IProcessInstance, TPropertyType>? getter = null, Action<IProcessInstance, TPropertyType>? setter = null)
    {
        if (_current == null) throw new Exception("There are not start point");

        if (!Nodes.ContainsKey(_current.To))
            Nodes[_current.To] = new NodeBuilderInfo
            {
                Name = _current.To, ElementType = _current.ToType, Parameters = new List<ParameterPersistenceInfo>()
            };

        Nodes[_current.To].Parameters.Add(new ParameterPersistenceInfo
        {
            Name = name,
            Getter = Unsafe.As<Func<IProcessInstance, object>?>(getter),
            Setter = Unsafe.As<Action<IProcessInstance, object>?>(setter),
            Type = typeof(TPropertyType)
        });
        return this;
    }
}