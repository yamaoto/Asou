using Asou.Core.Process.Binding;
using Microsoft.Extensions.Logging;

namespace Asou.GraphEngine.CodeContractStorage;

public class GraphProcessRegistration : IGraphProcessRegistration
{
    private readonly ILogger<GraphProcessRegistration> _logger;
    private readonly IParameterDelegateFactory _parameterDelegateFactory;
    private readonly IGraphProcessContractRepository _processContractRepository;

    public GraphProcessRegistration(
        IGraphProcessContractRepository processContractRepository,
        IParameterDelegateFactory parameterDelegateFactory,
        ILogger<GraphProcessRegistration> logger)
    {
        _processContractRepository = processContractRepository;
        _parameterDelegateFactory = parameterDelegateFactory;
        _logger = logger;
    }

    public bool RegisterFlow(GraphProcessContract graphProcessContract, bool validate = true, bool throwError = true)
    {
        var result = true;
        if (validate)
        {
            result = ValidateGraph(graphProcessContract, throwError);
        }
        // Prepare element parameters getter and setter delegates
        foreach (var (_, node) in graphProcessContract.Nodes)
        {
            foreach (var parameter in node.Parameters)
            {
                _parameterDelegateFactory.CreateDelegates(node.ElementType, parameter.Name);
            }
        }

        _processContractRepository.AddProcessContract(graphProcessContract);
        return result;
    }

    public bool ValidateGraph(GraphProcessContract graphProcessContract, bool throwError)
    {
        // Using quick-union algorithm with path compression (depth of any node is almost about 1 and pointing directly to the root)
        // https://en.wikipedia.org/wiki/Disjoint-set_data_structure
        var ids = new int[graphProcessContract.Nodes.Count];
        var map = new Dictionary<Guid, int>(graphProcessContract.Nodes.Count);
        for (var i = 0; i < ids.Length; i++)
        {
            ids[i] = i;
            var key = graphProcessContract.Nodes.Keys.ElementAt(i);
            map[graphProcessContract.Nodes[key].Id] = i;
        }

        // optimal initial capacity is 4
        var startPoints = new List<int>(4);
        foreach (var edge in graphProcessContract.Graph)
        {
            var to = map[edge.ToElementId];
            if (edge.FromElementId == null)
            {
                // start connection
                startPoints.Add(to);
                var newRoot = ids[to];
                if (newRoot != to)
                {
                    Union(ids, newRoot, to);
                }
            }
            else
            {
                var from = map[edge.FromElementId.Value];
                var n0 = GetRoot(ids, from);
                var n1 = GetRoot(ids, to);
                Union(ids, n0 < n1 ? n0 : n1, n0 < n1 ? n1 : n0);
            }
        }

        // Reorder graph root to new root with start point
        foreach (var point in startPoints)
        {
            if (ids[point] != point)
            {
                Union(ids, point, ids[point]);
            }
        }

        // check if all nodes are connected to start points
        var result = true;
        for (var i = 0; i < ids.Length; i++)
        {
            if (!startPoints.Contains(ids[i]))
            {
                var key = graphProcessContract.Nodes.Keys.ElementAt(i);
                if (throwError)
                {
                    throw new Exception("Graph is not connected. Element " + key + " is not connected to start point");
                }

                _logger.LogWarning("Graph is not connected. Element {ElementName} is not connected to start point",
                    key);
                result = false;
            }
        }

        return result;
    }

    private static int GetRoot(int[] ids, int node)
    {
        int root;
        if (ids[node] == node)
        {
            root = node;
        }
        else
        {
            root = GetRoot(ids, ids[node]);
            ids[node] = root;
        }

        return root;
    }

    private static int Union(int[] ids, int parent, int node)
    {
        if (ids[parent] == parent)
        {
            return ids[node] > parent ? ids[node] = parent : ids[parent] = node;
        }

        var newParent = Union(ids, ids[parent], node);
        return newParent;
    }
}
