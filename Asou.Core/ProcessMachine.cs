namespace Asou.Core;

public class ProcessMachine
{
    public Dictionary<string, object> Components { get; init; } = new();
    public Dictionary<string, object?> Parameters { get; init; } = new();
    public Dictionary<string, Action> Procedures { get; init; } = new();

    public required string Name { get; init; }
    public required ComponentFactoryMethod ComponentFactory { get; init; }

    #region Commands

    public void CreateComponent(string componentName, string name)
    {
        var component = ComponentFactory(componentName, componentName);
        Components[componentName] = component;
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

        var element = (BaseElement)Components[elementName];
        await element.ExecuteAsync(cancellationToken);
    }

    public async Task AfterExecuteElementAsync(string elementName, CancellationToken cancellationToken = default)
    {
        if (!Components.ContainsKey(elementName)) throw new InvalidOperationException();

        var element = (IAfterExecute)Components[elementName];
        await element.AfterExecuteAsync(cancellationToken);
    }

    public async Task ConfigureAwaiterAsync(string elementName, CancellationToken cancellationToken = default)
    {
        if (!Components.ContainsKey(elementName)) throw new InvalidOperationException();

        var element = (IAsyncExecutionElement)Components[elementName];
        await element.ConfigureAwaiterAsync(cancellationToken);
    }

    #endregion
}