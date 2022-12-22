using System.Runtime.CompilerServices;
using Asou.Abstractions;
using Asou.Abstractions.ExecutionElements;
using Asou.Core.Process.Binding;
using Asou.Core.Process.Delegates;

namespace Asou.Core.Process;

public class ProcessMachine : IProcessMachineCommands
{
    private readonly IParameterBinder _parameterBinder;
    private readonly Dictionary<string, BaseElement> _components = new ();
    private readonly Dictionary<string, object?> _parameters = new();
    private readonly Dictionary<string, Action> _procedures = new();

    public ProcessMachine(IParameterBinder parameterBinder, string name)
    {
        _parameterBinder = parameterBinder;
        Name = name;
    }

    public IReadOnlyDictionary<string, BaseElement> Components => _components;
    public IReadOnlyDictionary<string, object?> Parameters => _parameters;
    public IReadOnlyDictionary<string, Action> Procedures => _procedures;

    public required ComponentFactoryMethod ComponentFactory { get; init; }

    public string Name { get; init; }

    #region Commands

    public void CreateComponent(string componentName, string name)
    {
        var component = ComponentFactory(componentName, componentName);
        _components[componentName] = component;
    }

    public void LetParameter(string parameterName)
    {
        _parameters[parameterName] = new object();
    }

    public void DeleteParameter(string parameterName)
    {
        _parameters.Remove(parameterName);
    }

    public void SetParameter(string parameterName, AsouTypes parameterType, object? parameterValue)
    {
        _parameters[parameterName] = parameterValue;
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

    public async Task ConfigureAwaiterAsync(string elementName, CancellationToken cancellationToken = default)
    {
        if (!Components.ContainsKey(elementName)) throw new InvalidOperationException();

        var element = Unsafe.As<IAsyncExecutionElement>(Components[elementName]);
        await element.ConfigureAwaiterAsync(cancellationToken);
    }

    public void SetElementParameter<T>(string elementName, string parameterName, T value)
    {
        if (!Components.ContainsKey(elementName)) throw new InvalidOperationException();
        _parameterBinder.SetParameter(Components[elementName], parameterName, value);
    }

    public T GetElementParameter<T>(string elementName, string parameterName)
    {
        if (!Components.ContainsKey(elementName)) throw new InvalidOperationException();
        return _parameterBinder.GetParameter<T>(Components[elementName], parameterName);
    }

    #endregion
}