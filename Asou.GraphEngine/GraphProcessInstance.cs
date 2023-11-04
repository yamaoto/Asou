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
    private readonly IEventBus _eventBus;
    private readonly Queue<ExecutionElement> _executionQueue = new();
    private readonly IProcessExecutionLogRepository? _processExecutionLogRepository;
    private readonly IProcessExecutionPersistenceRepository? _processExecutionPersistenceRepository;
    private int _asynchronousResumeCount;
    private TaskCompletionSource? _queueTaskCompletionSource;


    public GraphProcessInstance(
        Guid id,
        ProcessContract processContract,
        ProcessRuntime processRuntime,
        ElementNode startNode,
        ElementNode[] nodes,
        PersistenceType persistenceType,
        ExecutionFlowType executionFlowType,
        IServiceScope serviceScope)
    {
        Id = id;
        ProcessContract = processContract;
        ProcessRuntime = processRuntime;
        StartNode = startNode;
        Nodes = nodes.ToDictionary(k => k.Id);
        PersistenceType = persistenceType;
        ExecutionFlowType = executionFlowType;
        _processExecutionLogRepository = serviceScope.ServiceProvider.GetService<IProcessExecutionLogRepository>();
        _processExecutionPersistenceRepository =
            serviceScope.ServiceProvider.GetService<IProcessExecutionPersistenceRepository>();
        _eventBus = serviceScope.ServiceProvider.GetRequiredService<IEventBus>();
    }

    public Guid ProcessId => ProcessContract.ProcessContractId;
    public Guid ProcessVersionId => ProcessContract.ProcessVersionId;
    public ElementNode StartNode { get; }
    public Dictionary<Guid, ElementNode> Nodes { get; }
    public PersistenceType PersistenceType { get; }
    public ExecutionFlowType ExecutionFlowType { get; }
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

        PrepareAndGetNodeElement(Nodes[subscription.ElementId]);

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
            await _eventBus.CancelSubscriptionAsync(subscription.Id, cancellationToken);

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

    public void PrepareStart(ExecutionOptions executionOptions)
    {
        _executionQueue.Enqueue(new ExecutionElement(StartNode.Id, Guid.NewGuid(), ExecutionStatuses.Execute));
    }


    /// <summary>
    ///     Prepare resume execution after system restart
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task PrepareResumeAsync(CancellationToken cancellationToken = default)
    {
        if (_processExecutionLogRepository == null)
        {
            throw new Exception("Type IProcessExecutionLogRepository not present in DI container");
        }
        // TODO: Restore _asynchronousResumeCount

        var threads = await _processExecutionLogRepository.GetThreadsAsync(Id, cancellationToken);
        foreach (var thread in threads)
        {
            var node = Nodes[thread.ElementId];
            _executionQueue.Enqueue(new ExecutionElement(thread.ElementId, thread.ThreadId,
                GetNextState(node, thread.State)));
        }
    }

    private async Task<int> MoveNextAsync(Guid threadId, ElementNode node, int state,
        CancellationToken cancellationToken = default)
    {
        PrepareAndGetNodeElement(node);

        if (state == ExecutionStatuses.Execute)
        {
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
        }

        if (state == ExecutionStatuses.AfterExecution)
        {
            await ProcessRuntime.AfterExecutionElementAsync(node.Id, cancellationToken);
        }

        if (state == ExecutionStatuses.ConfigureAsynchronousResume)
        {
            // Get subscriptions from element
            var subscriptions = await ProcessRuntime.ConfigureAsynchronousResumeAsync(node.Id, cancellationToken);
            var subscribed = false;
            foreach (var subscription in subscriptions)
            {
                await _eventBus.SubscribeAsync(Id, threadId, node.Id, subscription, cancellationToken);
                subscribed = true;
            }

            if (!subscribed)
            {
                // Break execution if no subscriptions
                // TODO: Log warning
                return ExecutionStatuses.Break;
            }
        }

        // TODO: Reduce code complexity: Remove GetNextState method and write direct instructions here
        return GetNextState(node, state);
    }

    private int GetNextState(ElementNode node, int state)
    {
        return state switch
        {
            ExecutionStatuses.Execute when node.UseAfterExecution => ExecutionStatuses.AfterExecution,
            ExecutionStatuses.Execute when !node.UseAfterExecution => ExecutionStatuses.Exit,
            ExecutionStatuses.AfterExecution when node.UseAsynchronousResume => ExecutionStatuses
                .ConfigureAsynchronousResume,
            ExecutionStatuses.AfterExecution when !node.UseAsynchronousResume => ExecutionStatuses.Exit,
            ExecutionStatuses.ConfigureAsynchronousResume => ExecutionStatuses.Break,
            _ => ExecutionStatuses.Exit
        };
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
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

                // TODO: If process execution flow type is asynchronous, save state and stop execution to continue after event fired
                if (ExecutionFlowType == ExecutionFlowType.Asynchronous)
                {
                    return;
                }

                // Wait new queue elements with TaskCompletionSource signal
                _queueTaskCompletionSource = new TaskCompletionSource();
                await _queueTaskCompletionSource.Task;
            }

            await DequeueAndExecuteAsync(cancellationToken);
        }
        // end of execution
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

        await SaveExecutionParameters(cancellationToken);

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
            (PersistenceType == PersistenceType.LogExecution && state == 0)
            || PersistenceType == PersistenceType.Stateful
        )
        {
            if (_processExecutionLogRepository == null)
            {
                throw new Exception("Type IProcessExecutionLogRepository not present in DI container");
            }

            await _processExecutionLogRepository.SaveExecutionStatusAsync(ProcessId, ProcessVersionId, Id,
                threadId, elementId, state);
        }
    }

    private async Task SaveExecutionParameters(CancellationToken cancellationToken)
    {
        if (PersistenceType == PersistenceType.Stateful)
        {
            if (_processExecutionPersistenceRepository == null)
            {
                throw new Exception("Type IProcessExecutionPersistenceRepository not present in DI container");
            }

            await _processExecutionPersistenceRepository.StoreProcessParameterAsync(Id,
                ProcessRuntime.Parameters.ToValueContainerMap(), cancellationToken);
        }
    }
}
