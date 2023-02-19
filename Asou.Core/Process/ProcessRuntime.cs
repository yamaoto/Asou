using System.Runtime.CompilerServices;
using Asou.Abstractions;
using Asou.Abstractions.Events;
using Asou.Abstractions.ExecutionElements;
using Asou.Abstractions.Process.Execution;
using Asou.Core.Process.Binding;
using Asou.Core.Process.Delegates;

namespace Asou.Core.Process;

public sealed class ProcessRuntime : IProcessRuntime
{
    private readonly Dictionary<Guid, BaseElement> _components = new();
    private readonly IParameterDelegateFactory _parameterDelegateFactory;

    public ProcessRuntime(
        IParameterDelegateFactory parameterDelegateFactory,
        string name)
    {
        _parameterDelegateFactory = parameterDelegateFactory;
        Name = name;
    }

    public required ComponentFactoryMethod ComponentFactory { get; init; }

    public IReadOnlyDictionary<Guid, BaseElement> Components => _components;
    public ProcessParameters Parameters { get; } = new();

    public string Name { get; init; }

    #region Commands

    public void CreateComponent(Guid componentId, Type componentType)
    {
        var component = ComponentFactory(componentType);
        _components[componentId] = component;
    }

    public void LetParameter(string parameterName)
    {
        Parameters[parameterName] = null;
    }

    public void DeleteParameter(string parameterName)
    {
        Parameters.Remove(parameterName);
    }

    public void SetParameter(string parameterName, AsouTypes parameterType, object? parameterValue)
    {
        Parameters[parameterName] = parameterValue;
    }

    public async Task ExecuteElementAsync(Guid elementId, CancellationToken cancellationToken = default)
    {
        if (!Components.ContainsKey(elementId))
        {
            throw new InvalidOperationException();
        }

        var element = Components[elementId];
        await element.ExecuteAsync(cancellationToken);
    }

    public async Task AfterExecutionElementAsync(Guid elementId, CancellationToken cancellationToken = default)
    {
        if (!Components.ContainsKey(elementId))
        {
            throw new InvalidOperationException();
        }

        var element = Unsafe.As<IAfterExecution>(Components[elementId]);
        await element.AfterExecutionAsync(cancellationToken);
    }

    public async Task<IEnumerable<EventSubscription>> ConfigureAsynchronousResumeAsync(Guid elementId,
        CancellationToken cancellationToken = default)
    {
        if (!Components.ContainsKey(elementId))
        {
            throw new InvalidOperationException();
        }

        var element = Unsafe.As<IAsynchronousResume>(Components[elementId]);
        var subscriptions = await element.ConfigureSubscriptionsAsync(cancellationToken);
        return subscriptions;
    }

    public async Task<bool> ValidateSubscriptionEventAsync(Guid elementId, EventRepresentation eventRepresentation,
        CancellationToken cancellationToken = default)
    {
        if (!Components.ContainsKey(elementId))
        {
            throw new InvalidOperationException();
        }

        var element = Unsafe.As<IAsynchronousResume>(Components[elementId]);
        var result = await element.ValidateSubscriptionEventAsync(eventRepresentation, cancellationToken);
        return result;
    }

    public void SetElementParameter(Guid elementId, string parameterName, object value)
    {
        if (!Components.ContainsKey(elementId))
        {
            throw new InvalidOperationException();
        }

        _parameterDelegateFactory.SetParameter(Components[elementId].GetType(),
            Components[elementId],
            parameterName, value);
    }

    public object GetElementParameter(Guid elementId, string parameterName)
    {
        if (!Components.ContainsKey(elementId))
        {
            throw new InvalidOperationException();
        }

        return _parameterDelegateFactory.GetParameter(Components[elementId].GetType(),
            Components[elementId],
            parameterName);
    }

    public object GetElementParameter<T>(Type elementType, Guid elementId, string parameterName) where T : class
    {
        if (!Components.ContainsKey(elementId))
        {
            throw new InvalidOperationException();
        }

        var getter = _parameterDelegateFactory.GetParameterGetter<T>(elementType, parameterName);
        return getter.Invoke(Unsafe.As<T>(Components[elementId]));
    }

    #endregion
}
