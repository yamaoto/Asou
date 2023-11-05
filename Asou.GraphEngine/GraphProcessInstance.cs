using Asou.Abstractions.Events;
using Asou.Abstractions.ExecutionElements;
using Asou.Abstractions.Process.Contract;
using Asou.Abstractions.Process.Execution;
using Asou.Abstractions.Process.Instance;
using Asou.Core.Process;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Asou.GraphEngine;

public class GraphProcessInstance : IProcessInstance
{
    private readonly IEventBus _eventBus;
    private readonly Queue<ExecutionElement> _executionQueue = new();
    private readonly ILogger<GraphProcessInstance> _logger;
    private readonly IProcessExecutionLogRepository? _processExecutionLogRepository;
    private readonly IProcessExecutionPersistenceRepository? _processExecutionPersistenceRepository;
    private int _asynchronousResumeCount;
    private TaskCompletionSource? _queueTaskCompletionSource;


    public GraphProcessInstance(
        Guid id,
        ProcessContract processContract,
        ProcessRuntime processRuntime,
        GraphElement start,
        GraphElement[] elements,
        PersistenceType persistenceType,
        ExecutionFlowType executionFlowType,
        IServiceScope serviceScope,
        ILogger<GraphProcessInstance> logger)
    {
        _logger = logger;
        Id = id;
        ProcessContract = processContract;
        ProcessRuntime = processRuntime;
        Start = start;
        Elements = elements.ToDictionary(k => k.Id);
        PersistenceType = persistenceType;
        ExecutionFlowType = executionFlowType;
        _processExecutionLogRepository = serviceScope.ServiceProvider.GetService<IProcessExecutionLogRepository>();
        _processExecutionPersistenceRepository =
            serviceScope.ServiceProvider.GetService<IProcessExecutionPersistenceRepository>();
        _eventBus = serviceScope.ServiceProvider.GetRequiredService<IEventBus>();
    }

    public Guid ProcessId => ProcessContract.ProcessContractId;
    public Guid ProcessVersionId => ProcessContract.ProcessVersionId;
    public GraphElement Start { get; }
    public Dictionary<Guid, GraphElement> Elements { get; }
    public PersistenceType PersistenceType { get; }
    public ExecutionFlowType ExecutionFlowType { get; }
    public ProcessContract ProcessContract { get; }

    public Guid Id { get; init; }

    public IProcessRuntime ProcessRuntime { get; }


    /// <summary>
    ///     Handle events from subscriptions
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="subscription"></param>
    /// <param name="eventRepresentation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task HandleSubscriptionEventAsync(EventSubscriptionModel subscription,
        EventRepresentation eventRepresentation, CancellationToken cancellationToken = default)
    {
        if (!Elements.TryGetValue(subscription.ElementId, out var element))
        {
            return;
        }

        PrepareAndGetElement(Elements[subscription.ElementId]);

        // Handle events: asynchronous resume
        if (subscription.EventSubscriptionType == EventSubscriptionType.AsynchronousResume)
        {
            // Validate event in element
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
            _executionQueue.Enqueue(new ExecutionElement(element.Id, subscription.ThreadId, ExecutionStatuses.Exit));
            if (_queueTaskCompletionSource != null && !_queueTaskCompletionSource.TrySetResult())
            {
                // TODO: Rewrite Task Completion Source to distinguish execution thread
                _logger.LogWarning(
                    "HandleSubscriptionEventAsync: AsynchronousResume task completion synchronization error");
            }
        }

        // Handle events: send events to process
        if (subscription.EventSubscriptionType == EventSubscriptionType.HandleEvent)
        {
            // TODO: Handle message directly in element
            // If execution element is can handle events directly by implementing new interface (something like Asou.Abstractions.ExecutionElements.IDispatchEvent)
            // then call this interface method. For this task should be added new method to IProcessRuntime interface
            throw new NotImplementedException();
        }
    }

    public void PrepareStart(ExecutionOptions executionOptions)
    {
        _executionQueue.Enqueue(new ExecutionElement(Start.Id, Guid.NewGuid(), ExecutionStatuses.Execute));
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
        // TODO: Restore _asynchronousResumeCount to handle correct process stop

        var threads = await _processExecutionLogRepository.GetThreadsAsync(Id, cancellationToken);
        foreach (var thread in threads)
        {
            var element = Elements[thread.ElementId];
            var nextState = GetNextState(element, thread.State);
            var nextExecutionElement = new ExecutionElement(thread.ElementId, thread.ThreadId, nextState);
            _executionQueue.Enqueue(nextExecutionElement);
        }
    }

    private async Task<int> MoveNextAsync(Guid threadId, GraphElement element, int state,
        CancellationToken cancellationToken = default)
    {
        PrepareAndGetElement(element);

        if (state == ExecutionStatuses.Execute)
        {
            // set declared parameters value from they're getters
            foreach (var parameter in element.Parameters.Where(w => w.Getter != null))
            {
                var value = parameter.Getter!.Invoke(this);

                // TODO: there are may be some possible cases of boxing. Should to write implicit for value types
                ProcessRuntime.SetElementParameter(element.Id, parameter.Name, value);
            }

            await ProcessRuntime.ExecuteElementAsync(element.Id, cancellationToken);

            // get and store parameter values by they're setters
            foreach (var parameter in element.Parameters.Where(w => w.Setter != null))
            {
                var value = ProcessRuntime.GetElementParameter(element.Id, parameter.Name);

                // TODO: there are may be some possible cases of boxing. Should to write implicit for value types
                parameter.Setter!.Invoke(this, value);
            }

            // TODO: Update parameters with binding behavior to keep track of changes
        }

        if (state == ExecutionStatuses.AfterExecution)
        {
            await ProcessRuntime.AfterExecutionElementAsync(element.Id, cancellationToken);
        }

        if (state == ExecutionStatuses.ConfigureAsynchronousResume)
        {
            // Get subscriptions from element
            var subscriptions = await ProcessRuntime.ConfigureAsynchronousResumeAsync(element.Id, cancellationToken);
            var subscribed = false;
            foreach (var subscription in subscriptions)
            {
                await _eventBus.SubscribeAsync(Id, threadId, element.Id, subscription, cancellationToken);
                subscribed = true;
            }

            if (!subscribed)
            {
                _logger.LogWarning(
                    "ConfigureAsynchronousResume: Process instance {ProcessInstanceId} element {ElementId} has no subscriptions to resume execution and can't be resumed",
                    Id, element.Id);
            }
        }

        return GetNextState(element, state);
    }

    private static int GetNextState(GraphElement element, int state)
    {
        return state switch
        {
            ExecutionStatuses.Execute when element.UseAfterExecution => ExecutionStatuses.AfterExecution,
            ExecutionStatuses.Execute when !element.UseAfterExecution => ExecutionStatuses.Exit,
            ExecutionStatuses.AfterExecution when element.UseAsynchronousResume => ExecutionStatuses
                .ConfigureAsynchronousResume,
            ExecutionStatuses.AfterExecution when !element.UseAsynchronousResume => ExecutionStatuses.Exit,
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

                if (ExecutionFlowType == ExecutionFlowType.Asynchronous)
                {
                    // If process execution flow type is asynchronous, save state and stop execution to continue after event fired
                    // TODO: Ensure that process instance state is saved
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
        var current = Elements[execution.ElementId];
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

            // Break execution of current element
            return;
        }

        // Calculate next execution element
        EnqueueNextExecution(execution);
    }

    private void EnqueueNextExecution(ExecutionElement execution)
    {
        // If gate is inclusive (bpmn x) this means that only one element will be executed
        // If gate is exclusive (bpmn +), all positive connections will be executed
        var inclusiveFound = false;
        var exit = false;
        var currentThreadFound = false;
        var element = Elements[execution.ElementId];
        foreach (var edge in element.Connections)
        {
            if (edge is ConditionalConnection conditionalConnection)
            {
                var from = ProcessRuntime.Components[execution.ElementId];
                var to = PrepareAndGetElement(edge.To);
                if (!conditionalConnection.IsCanNavigate(this, from, to))
                {
                    continue;
                }

                exit = true;
            }
            else
            {
                if (element.IsInclusiveGate && !inclusiveFound)
                {
                    exit = true;
                }
            }

            var next = edge.To;
            var threadId = currentThreadFound ? Guid.NewGuid() : execution.ThreadId;
            _executionQueue.Enqueue(new ExecutionElement(next.Id, threadId, ExecutionStatuses.Execute));
            currentThreadFound = true;
            inclusiveFound = true;
            if (!element.IsInclusiveGate || exit)
            {
                break;
            }
        }
    }

    private BaseElement PrepareAndGetElement(GraphElement element)
    {
        if (!ProcessRuntime.Components.ContainsKey(element.Id))
        {
            ProcessRuntime.CreateComponent(element.Id, element.ElementType);
        }

        return ProcessRuntime.Components[element.Id];
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
