using Asou.Abstractions;
using Asou.Abstractions.Events;
using Asou.Abstractions.ExecutionElements;
using Asou.Core;
using Asou.Core.Process;
using Microsoft.Extensions.DependencyInjection;

namespace Asou.GraphEngine;

public class GraphProcessInstance : IProcessInstance
{
    private readonly IExecutionPersistence? _executionPersistence;
    private readonly IEventDriver _eventDriver;
    private readonly IServiceScope _serviceScope;
    private readonly Dictionary<Guid, Guid> _threads = new();
    private readonly Dictionary<string, int> _nodeMap = new();
    private Queue<Tuple<ElementNodeId, int>> _executionQueue = new();
    private TaskCompletionSource _queueTaskCompletionSource;
    private int _awaitingSubscriptions;


    public GraphProcessInstance(
        Guid id,
        ProcessContract processContract,
        ProcessRuntime processRuntime,
        ElementNode startNode,
        ElementNode[] nodes,
        IServiceScope serviceScope)
    {
        Id = id;
        ProcessContract = processContract;
        ProcessRuntime = processRuntime;
        _serviceScope = serviceScope;
        StartNode = startNode;
        Nodes = nodes;
        for (int i = 0; i < nodes.Length; i++)
        {
            _nodeMap.Add(nodes[i].Name, i);
        }
        _executionPersistence = serviceScope.ServiceProvider.GetService<IExecutionPersistence>();
        _eventDriver = serviceScope.ServiceProvider.GetRequiredService<IEventDriver>();
    }

    public Guid ProcessId => ProcessContract.ProcessContractId;
    public Guid ProcessVersionId => ProcessContract.ProcessVersionId;
    public ElementNode StartNode { get; init; }
    public ElementNode[] Nodes { get; init; }
    public PersistType PersistType { get; init; }

    public ProcessContract ProcessContract { get; }

    public Guid Id { get; init; }

    public ProcessRuntime ProcessRuntime { get; }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(Guid.NewGuid(), StartNode, ExecutionStatuses.Execute, cancellationToken);
    }

    public Task ResumeAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Restore _awaitingSubscriptions
        // TODO: Enqueue next
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Handle events from subscriptions
    /// </summary>
    /// <remarks>
    ///     Caller must guarantee <see cref="elementName" /> currently in executions steps 
    /// </remakrs>
    /// <param name="threadId"></param>
    /// <param name="elementName"></param>
    /// <param name="eventRepresentation"></param>
    /// <param name="eventSubscriptionType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task HandleSubscriptionEventAsync(Guid subscriptionId, Guid threadId, string elementName, EventRepresentation eventRepresentation, EventSubscriptionType eventSubscriptionType, CancellationToken cancellationToken = default)
    {
        if (!_nodeMap.TryGetValue(elementName, out var pos))
        {
            return;
        }
        var node = Nodes[pos];

        if (eventSubscriptionType == EventSubscriptionType.AsyncExecutionResumer)
        {
            // Validate event in node
            var result = await ProcessRuntime.ValidateSubscriptionEventAsync(elementName, eventRepresentation);
            if (!result)
            {
                return;
            }
            // Unsubscribe from event subscription
            await _eventDriver.CancelSubscriptionAsync(subscriptionId, cancellationToken);

            // Decrease counter to handle correct process stop
            _awaitingSubscriptions--;

            // Resume execution
            _executionQueue.Enqueue(new(new ElementNodeId(threadId, node), ExecutionStatuses.Exit));
            _queueTaskCompletionSource.SetResult();
        }

        if (eventSubscriptionType == EventSubscriptionType.EventHandler)
        {
            // TODO: Handle message in node
            throw new NotImplementedException();
        }
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
            // Get subscriptions fron element
            var subscriptions = await ProcessRuntime.ConfigureAwaiterAsync(node.Name, cancellationToken);
            var isShouldBreak = false;
            foreach (var subscription in subscriptions)
            {
                await _eventDriver.SubscribeAsync(Id, node.Name, subscription, cancellationToken);
                isShouldBreak = true;
            }

            // Break execution if collection is't empty subscriptions 
            // Continue if subscription collection is empty
            return isShouldBreak ? ExecutionStatuses.Break : ExecutionStatuses.Exit;
        }

        return ExecutionStatuses.Exit;
    }

    private async Task ExecuteAsync(Guid threadId, ElementNode node, int initialState = ExecutionStatuses.Execute, CancellationToken cancellationToken = default)
    {
        _executionQueue.Enqueue(new(new ElementNodeId(threadId, node), initialState));
        while (true)
        {
            if (_executionQueue.Count == 0)
            {
                if (_awaitingSubscriptions == 0)
                {
                    // Property _awaitingSubscriptions uses to handle correct process stop
                    // If nothing await process should to stop execution
                    break;
                }

                // Wait new queue elements with TaskCompletionSource signal
                _queueTaskCompletionSource = new TaskCompletionSource();
                await _queueTaskCompletionSource.Task;
            }
            await DequeueAndExecuteAsync();
        }
    }

    private async Task DequeueAndExecuteAsync(CancellationToken cancellationToken = default)
    {
        var tuple = _executionQueue.Dequeue();
        var current = tuple.Item1;
        var state = tuple.Item2;
        while (state != ExecutionStatuses.Exit && state != ExecutionStatuses.Break)
        {
            await SaveExecutionStatus(current.Id, current.Node.Name, state);

            state = await MoveNextAsync(current.Node, state, cancellationToken);
        }
        if (state == ExecutionStatuses.Break)
        {
            // Increase counter to handle correct process stop
            _awaitingSubscriptions++;

            // Break execution of current node
            return;
        }

        // Calculate next execution node
        EnqueueNextExecution(current);
    }

    private void EnqueueNextExecution(ElementNodeId current)
    {
        // If gate is inclusive (bpmn x) this means taht only one node will be executed
        // If gate is exclusive (bpmn +), all positive connections will be executed
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
            var threadId = currentThreadFound ? Guid.NewGuid() : current.Id;
            _executionQueue.Enqueue(new(new ElementNodeId(threadId, next), ExecutionStatuses.Execute));
            currentThreadFound = true;
            inclusiveFound = true;
            if (!current.Node.IsInclusiveGate || exit) break;
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
        if (
            (PersistType == PersistType.ElemExec && state == 0)
            || (PersistType == PersistType.ElemStateExec)
        )
        {
            if (_executionPersistence == null)
            {
                throw new Exception("Type IExecutionPersistence not present in DI container");
            }
            await _executionPersistence.SaveExecutionStatus(ProcessId, ProcessVersionId, Id,
            threadId,
            elementName, state);
        }
    }
}