namespace Asou.Abstractions;

public interface IProcessMachineCommands : IProcessMachine
{
    void CreateComponent(string componentName, string name);
    void LetParameter(string parameterName);
    void DeleteParameter(string parameterName);
    void SetParameter(string parameterName, AsouTypes parameterType, object? parameterValue);
    void CallProcedure(string procedureName);
    Task ExecuteElementAsync(string elementName, CancellationToken cancellationToken = default);
    Task AfterExecuteElementAsync(string elementName, CancellationToken cancellationToken = default);
    Task ConfigureAwaiterAsync(string elementName, CancellationToken cancellationToken = default);
    void SetElementParameter(string elementName, string parameterName, object value);
    object GetElementParameter(string elementName, string parameterName);
    object GetElementParameter<T>(Type elementType, string elementName, string parameterName) where T : class;
}