using System.Runtime.CompilerServices;
using Asou.Abstractions;
using Asou.Abstractions.Events;
using Asou.Abstractions.ExecutionElements;
using Asou.Core.Process.Binding;
using Asou.Core.Process.Delegates;

namespace Asou.Core.Process;

public sealed class ProcessRuntime : IProcessMachineCommands
{
    private readonly Dictionary<string, BaseElement> _components = new();
    private readonly IParameterDelegateFactory _parameterDelegateFactory;
    private readonly Dictionary<string, Action> _procedures = new();

    public ProcessRuntime(
        IParameterDelegateFactory parameterDelegateFactory,
        string name)
    {
        _parameterDelegateFactory = parameterDelegateFactory;
        Name = name;
    }

    public required ComponentFactoryMethod ComponentFactory { get; init; }

    public IReadOnlyDictionary<string, BaseElement> Components => _components;
    public ProcessParameters Parameters { get; } = new();

    public IReadOnlyDictionary<string, Action> Procedures => _procedures;

    public string Name { get; init; }

    #region Commands

    public void CreateComponent(string componentName, string name)
    {
        var component = ComponentFactory(componentName, componentName);
        _components[componentName] = component;
    }

    public void LetParameter(string parameterName)
    {
        Parameters[parameterName] = new object();
    }

    public void DeleteParameter(string parameterName)
    {
        Parameters.Remove(parameterName);
    }

    public void SetParameter(string parameterName, AsouTypes parameterType, object? parameterValue)
    {
        Parameters[parameterName] = parameterValue;
    }

    public void CallProcedure(string procedureName)
    {
        if (!Procedures.ContainsKey(procedureName)) throw new InvalidOperationException();

        Procedures[procedureName]();
    }

    public async Task ExecuteElementAsync(string elementName, CancellationToken cancellationToken = default)
    {
        if (!Components.ContainsKey(elementName)) throw new InvalidOperationException();

        var element = Components[elementName];
        await element.ExecuteAsync(cancellationToken);
    }

    public async Task AfterExecuteElementAsync(string elementName, CancellationToken cancellationToken = default)
    {
        if (!Components.ContainsKey(elementName)) throw new InvalidOperationException();

        var element = Unsafe.As<IAfterExecute>(Components[elementName]);
        await element.AfterExecuteAsync(cancellationToken);
    }

    public async Task<IEnumerable<EventSubscription>> ConfigureAwaiterAsync(string elementName, CancellationToken cancellationToken = default)
    {
        if (!Components.ContainsKey(elementName)) throw new InvalidOperationException();

        var element = Unsafe.As<IAsyncExecutionElement>(Components[elementName]);
        var subscriptions = await element.ConfigureAwaiterAsync(cancellationToken);
        return subscriptions;
    }

    public void SetElementParameter(string elementName, string parameterName, object value)
    {
        if (!Components.ContainsKey(elementName)) throw new InvalidOperationException();

        _parameterDelegateFactory.SetParameter(Components[elementName].GetType(),
            Components[elementName],
            parameterName, value);
    }

    public object GetElementParameter(string elementName, string parameterName)
    {
        if (!Components.ContainsKey(elementName)) throw new InvalidOperationException();

        return _parameterDelegateFactory.GetParameter(Components[elementName].GetType(),
            Components[elementName],
            parameterName);
    }

    public object GetElementParameter<T>(Type elementType, string elementName, string parameterName) where T : class
    {
        if (!Components.ContainsKey(elementName)) throw new InvalidOperationException();

        var getter = _parameterDelegateFactory.GetParameterGetter<T>(elementType, parameterName);
        return getter.Invoke(Unsafe.As<T>(Components[elementName]));
    }

    #endregion
}