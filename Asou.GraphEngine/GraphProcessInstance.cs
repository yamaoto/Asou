using Asou.Abstractions.ExecutionElements;
using Asou.Core;
using Asou.Core.Process;
using Microsoft.Extensions.DependencyInjection;

namespace Asou.GraphEngine;

public class GraphProcessInstance : IProcessInstance
{
    private readonly IExecutionPersistence _executionPersistence;
    private readonly IServiceScope _serviceScope;
    private readonly Dictionary<Guid, Guid> _threads = new();

    public GraphProcessInstance(
        Guid id,
        ProcessContract processContract,
        ProcessRuntime processRuntime,
        ElementNode startNode,
        List<ElementNode> nodes,
        IServiceScope serviceScope)
    {
        Id = id;
        ProcessContract = processContract;
        ProcessRuntime = processRuntime;
        _serviceScope = serviceScope;
        StartNode = startNode;
        Nodes = nodes;
        _executionPersistence = serviceScope.ServiceProvider.GetRequiredService<IExecutionPersistence>();
    }

    public Guid ProcessId => ProcessContract.ProcessContractId;
    public Guid ProcessVersionId => ProcessContract.ProcessVersionId;
    public ElementNode StartNode { get; init; }
    public List<ElementNode> Nodes { get; init; }
    public PersistType PersistType { get; init; }

    public ProcessContract ProcessContract { get; }

    public Guid Id { get; init; }

    public ProcessRuntime ProcessRuntime { get; }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(Guid.NewGuid(), StartNode, cancellationToken);
    }


    private async Task<int> MoveNextAsync(ElementNode node, int state, CancellationToken cancellationToken = default)
    {
        if (state == ExecutionStatuses.Execute)
        {
            PrepareAndGetNodeElement(node);

            // set declared parameters value from they're getters 
            if (node.Parameters != null)
                foreach (var parameter in node.Parameters.Where(w => w.Getter != null))
                {
                    var value = parameter.Getter!.Invoke(this);

                    // TODO: there are may be some possible cases of boxing. Should to write implicit for value types
                    ProcessRuntime.SetElementParameter(node.Name, parameter.Name, value);
                }

            await ProcessRuntime.ExecuteElementAsync(node.Name, cancellationToken);

            // get and store parameter values by they're setters 
            if (node.Parameters != null)
                foreach (var parameter in node.Parameters.Where(w => w.Setter != null))
                {
                    var value = ProcessRuntime.GetElementParameter(node.Name, parameter.Name);

                    // TODO: there are may be some possible cases of boxing. Should to write implicit for value types
                    parameter.Setter!.Invoke(this, value);
                }

            //TODO: parameters binding set

            return node.IsShouldCallAfterExecute ? ExecutionStatuses.AfterExecute : ExecutionStatuses.Exit;
        }

        if (state == ExecutionStatuses.AfterExecute)
        {
            await ProcessRuntime.AfterExecuteElementAsync(node.Name, cancellationToken);

            return node.IsShouldAwait ? ExecutionStatuses.ConfigureAwaiter : ExecutionStatuses.Exit;
        }

        if (state == ExecutionStatuses.ConfigureAwaiter)
        {
            await ProcessRuntime.ConfigureAwaiterAsync(node.Name, cancellationToken);
            return ExecutionStatuses.Exit;
        }

        return ExecutionStatuses.Exit;
    }

    private async Task ExecuteAsync(Guid threadId, ElementNode node, CancellationToken cancellationToken = default)
    {
        var queue = new Queue<ElementNodeId>();
        queue.Enqueue(new ElementNodeId(threadId, node));
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var state = 0;
            while (state != ExecutionStatuses.Exit)
            {
                await SaveExecutionStatus(current.Id, current.Node.Name, state);

                state = await MoveNextAsync(current.Node, state, cancellationToken);
            }


            var inclusiveFound = false;
            var exit = false;
            var currentThreadFound = false;
            for (var i = 0; i < current.Node.Connections.Count; i++)
            {
                var edge = current.Node.Connections[i];
                if (edge is ConditionalConnection conditionalElementNodeConnection)
                {
                    var from = ProcessRuntime.Components[current.Node.Name];
                    var to = PrepareAndGetNodeElement(edge.To);
                    if (!conditionalElementNodeConnection.IsCanNavigate(this, from, to)) continue;

                    exit = true;
                }
                else
                {
                    if (current.Node.IsInclusiveGate && !inclusiveFound) exit = true;
                }

                var next = edge.To;
                var thread = currentThreadFound ? Guid.NewGuid() : current.Id;
                queue.Enqueue(new ElementNodeId(thread, next));
                currentThreadFound = true;
                inclusiveFound = true;
                if (!current.Node.IsInclusiveGate || exit) break;
            }
        }
    }

    private BaseElement PrepareAndGetNodeElement(ElementNode node)
    {
        if (!ProcessRuntime.Components.ContainsKey(node.Name))
            ProcessRuntime.CreateComponent(node.Type.Name, node.Name);

        return ProcessRuntime.Components[node.Name];
    }


    private async Task SaveExecutionStatus(Guid threadId,
        string elementName, int state)
    {
        if (PersistType == PersistType.ElemExec && state == 0)
            await _executionPersistence.SaveExecutionStatus(ProcessId, ProcessVersionId, Id,
                threadId,
                elementName, state);

        if (PersistType == PersistType.ElemStateExec)
            await _executionPersistence.SaveExecutionStatus(ProcessId, ProcessVersionId, Id,
                threadId,
                elementName, state);
    }
}