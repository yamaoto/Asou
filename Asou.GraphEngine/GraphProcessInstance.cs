using Asou.Abstractions.Events;
using Asou.Abstractions.ExecutionElements;
using Asou.Abstractions.Process.Contract;
using Asou.Abstractions.Process.Execution;
using Asou.Abstractions.Process.Instance;
using Asou.Core.Process;
using Microsoft.Extensions.DependencyInjection;

namespace Asou.GraphEngine;

public class GraphProcessInstance : IProcessInstance
{
    private readonly IEventDriver _eventDriver;
    private readonly Queue<ExecutionElement> _executionQueue = new();
    private readonly IProcessExecutionLogRepository? _processExecutionLogRepository;
    private int _asynchronousResumeCount;
    private TaskCompletionSource? _queueTaskCompletionSource;


    public GraphProcessInstance(
        Guid id,
        ProcessContract processContract,
        ProcessRuntime processRuntime,
        ElementNode startNode,
        ElementNode[] nodes,
        PersistenceType persistenceType,
        IServiceScope serviceScope)
    {
        Id = id;
        ProcessContract = processContract;
        ProcessRuntime = processRuntime;
        StartNode = startNode;
        Nodes = nodes.ToDictionary(k => k.Id);
        PersistenceType = persistenceType;
        _processExecutionLogRepository = serviceScope.ServiceProvider.GetService<IProcessExecutionLogRepository>();
        _eventDriver = serviceScope.ServiceProvider.GetRequiredService<IEventDriver>();
    }

    public Guid ProcessId => ProcessContract.ProcessContractId;
    public Guid ProcessVersionId => ProcessContract.ProcessVersionId;
    public ElementNode StartNode { get; }
    public Dictionary<Guid, ElementNode> Nodes { get; }
    public PersistenceType PersistenceType { get; }

    public ProcessContract ProcessContract { get; }

    public Guid Id { get; init; }

    public IProcessRuntime ProcessRuntime { get; }


    /// <summary>
    ///     Handle events from subscriptions
    /// </summary>
    /// <remarks>
    ///     Caller must guarantee <see cref="elementName" /> currently in executions steps
    ///     </remakrs>
    ///     <param name="subscription"></param>
    ///     <param name="eventRepresentation"></param>
    ///     <param name="cancellationToken"></param>
    ///     <returns></returns>
    public async Task HandleSubscriptionEventAsync(EventSubscriptionModel subscription,
        EventRepresentation eventRepresentation, CancellationToken cancellationToken = default)
    {
        if (!Nodes.TryGetValue(subscription.ElementId, out var node))
        {
            return;
        }

        // Handle events: asynchronous resume
        if (subscription.EventSubscriptionType == EventSubscriptionType.AsynchronousResume)
        {
            // Validate event in node
            var result =
                await ProcessRuntime.ValidateSubscriptionEventAsync(subscription.ElementId, eventRepresentation,
                    cancellationToken);
            if (!result)
            {
                return;
            }

            // Unsubscribe from event subscription
            await _eventDriver.CancelSubscriptionAsync(subscription.Id, cancellationToken);

            // Decrease counter to handle correct process stop
            _asynchronousResumeCount--;

            // Resume execution
            _executionQueue.Enqueue(new ExecutionElement(node.Id, subscription.ThreadId, ExecutionStatuses.Exit));
            if (_queueTaskCompletionSource != null && !_queueTaskCompletionSource.TrySetResult())
            {
                // TODO: Log warning
            }
        }

        // Handle events: send events to process
        if (subscription.EventSubscriptionType == EventSubscriptionType.HandleEvent)
            // TODO: Handle message in node
        {
            throw new NotImplementedException();
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(Guid.NewGuid(), StartNode, ExecutionStatuses.Execute, cancellationToken);
    }

    /// <summary>
    ///     Resume execution after system restart
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task ResumeAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Restore _asynchronousResumeCount
        // TODO: Enqueue next
        throw new NotImplementedException();
    }

    private async Task<int> MoveNextAsync(Guid threadId, ElementNode node, int state,
        CancellationToken cancellationToken = default)
    {
        if (state == ExecutionStatuses.Execute)
        {
            PrepareAndGetNodeElement(node);

            // set declared parameters value from they're getters
            foreach (var parameter in node.Parameters.Where(w => w.Getter != null))
            {
                var value = parameter.Getter!.Invoke(this);

                // TODO: there are may be some possible cases of boxing. Should to write implicit for value types
                ProcessRuntime.SetElementParameter(node.Id, parameter.Name, value);
            }

            await ProcessRuntime.ExecuteElementAsync(node.Id, cancellationToken);

            // get and store parameter values by they're setters
            foreach (var parameter in node.Parameters.Where(w => w.Setter != null))
            {
                var value = ProcessRuntime.GetElementParameter(node.Id, parameter.Name);

                // TODO: there are may be some possible cases of boxing. Should to write implicit for value types
                parameter.Setter!.Invoke(this, value);
            }

            //TODO: parameters binding set

            return node.UseAfterExecution ? ExecutionStatuses.AfterExecution : ExecutionStatuses.Exit;
        }

        if (state == ExecutionStatuses.AfterExecution)
        {
            await ProcessRuntime.AfterExecutionElementAsync(node.Id, cancellationToken);

            return node.UseAsynchronousResume ? ExecutionStatuses.ConfigureAsynchronousResume : ExecutionStatuses.Exit;
        }

        if (state == ExecutionStatuses.ConfigureAsynchronousResume)
        {
            // Get subscriptions from element
            var subscriptions = await ProcessRuntime.ConfigureAsynchronousResumeAsync(node.Id, cancellationToken);
            var isShouldBreak = false;
            foreach (var subscription in subscriptions)
            {
                await _eventDriver.SubscribeAsync(Id, threadId, node.Id, subscription, cancellationToken);
                isShouldBreak = true;
            }

            // Break execution if collection isn't empty subscriptions
            // Continue if subscription collection is empty
            return isShouldBreak ? ExecutionStatuses.Break : ExecutionStatuses.Exit;
        }

        return ExecutionStatuses.Exit;
    }

    private async Task ExecuteAsync(Guid threadId, ElementNode node, int initialState = ExecutionStatuses.Execute,
        CancellationToken cancellationToken = default)
    {
        _executionQueue.Enqueue(new ExecutionElement(node.Id, threadId, initialState));
        while (true)
        {
            if (_executionQueue.Count == 0)
            {
                if (_asynchronousResumeCount == 0)
                    // Property _asynchronousResumeCount uses to handle correct process stop
                    // If nothing to resume, process should to stop execution
                {
                    break;
                }

                // Wait new queue elements with TaskCompletionSource signal
                _queueTaskCompletionSource = new TaskCompletionSource();
                await _queueTaskCompletionSource.Task;
            }

            await DequeueAndExecuteAsync(cancellationToken);
        }
        // end of execution
        // TODO: Inform ProcessExecutionEngine about execution stopping
    }

    private async Task DequeueAndExecuteAsync(CancellationToken cancellationToken = default)
    {
        var execution = _executionQueue.Dequeue();
        var current = Nodes[execution.ElementId];
        var state = execution.ExecutionState;
        while (state != ExecutionStatuses.Exit && state != ExecutionStatuses.Break)
        {
            await SaveExecutionStatus(execution.ThreadId, execution.ElementId, state);

            state = await MoveNextAsync(execution.ThreadId, current, state, cancellationToken);
        }

        if (state == ExecutionStatuses.Break)
        {
            // Increase counter to handle correct process stop
            _asynchronousResumeCount++;

            // Break execution of current node
            return;
        }

        // Calculate next execution node
        EnqueueNextExecution(execution);
    }

    private void EnqueueNextExecution(ExecutionElement execution)
    {
        // If gate is inclusive (bpmn x) this means that only one node will be executed
        // If gate is exclusive (bpmn +), all positive connections will be executed
        var inclusiveFound = false;
        var exit = false;
        var currentThreadFound = false;
        var node = Nodes[execution.ElementId];
        foreach (var edge in node.Connections)
        {
            if (edge is ConditionalConnection conditionalElementNodeConnection)
            {
                var from = ProcessRuntime.Components[execution.ElementId];
                var to = PrepareAndGetNodeElement(edge.To);
                if (!conditionalElementNodeConnection.IsCanNavigate(this, from, to))
                {
                    continue;
                }

                exit = true;
            }
            else
            {
                if (node.IsInclusiveGate && !inclusiveFound)
                {
                    exit = true;
                }
            }

            var next = edge.To;
            var threadId = currentThreadFound ? Guid.NewGuid() : execution.ThreadId;
            _executionQueue.Enqueue(new ExecutionElement(next.Id, threadId, ExecutionStatuses.Execute));
            currentThreadFound = true;
            inclusiveFound = true;
            if (!node.IsInclusiveGate || exit)
            {
                break;
            }
        }
    }

    private BaseElement PrepareAndGetNodeElement(ElementNode node)
    {
        if (!ProcessRuntime.Components.ContainsKey(node.Id))
        {
            ProcessRuntime.CreateComponent(node.Id, node.ElementType);
        }

        return ProcessRuntime.Components[node.Id];
    }

    private async Task SaveExecutionStatus(Guid threadId,
        Guid elementId, int state)
    {
        if (
            (PersistenceType == PersistenceType.StandardPersistence && state == 0)
            || PersistenceType == PersistenceType.AdditionalPersistence
        )
        {
            if (_processExecutionLogRepository == null)
            {
                throw new Exception("Type IExecutionPersistence not present in DI container");
            }

            await _processExecutionLogRepository.SaveExecutionStatusAsync(ProcessId, ProcessVersionId, Id,
                threadId, elementId, state);
        }
    }
}
