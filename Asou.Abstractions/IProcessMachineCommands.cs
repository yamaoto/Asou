namespace Asou.Abstractions;

/// <summary>Processes commands from the process state machine.</summary>
/// <remarks>This interface is by process engines to communicate with process state machine.</remarks>
public interface IProcessMachineCommands : IProcessMachine
{
    /// <summary>Creates a component.</summary>
    /// <param name="componentName">The name of the component to create.</param>
    /// <param name="name">The name of the component.</param>
    void CreateComponent(string componentName, string name);

    /// <summary>Let the parameter name be used.</summary>
    /// <param name="parameterName">The name of the parameter.</param>
    void LetParameter(string parameterName);
    
    /// <summary>Deletes the specified parameter.</summary>
    /// <param name="parameterName">The name of the parameter to delete.</param>
    void DeleteParameter(string parameterName);

    /// <summary>Sets a parameter.</summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="parameterType">The type of the parameter.</param>
    /// <param name="parameterValue">The value of the parameter.</param>
    void SetParameter(string parameterName, AsouTypes parameterType, object? parameterValue);

    /// <summary>Calls a procedure.</summary>
    /// <param name="procedureName">The name of the procedure to call.</param>
    void CallProcedure(string procedureName);

    /// <summary>Executes an element in the current process.</summary>
    /// <param name="elementName">The name of the element to execute.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the operation.</returns>
    Task ExecuteElementAsync(string elementName, CancellationToken cancellationToken = default);
    
    /// <summary>Called after an element is executed.</summary>
    /// <param name="elementName">The name of the element.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the operation.</returns>
    Task AfterExecuteElementAsync(string elementName, CancellationToken cancellationToken = default);
    
    /// <summary>Configures the persistant awaiter for the specified element.</summary>
    /// <param name="elementName">The name of the element.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the operation.</returns>
    Task ConfigureAwaiterAsync(string elementName, CancellationToken cancellationToken = default);
    
    /// <summary>Sets the value of a parameter on an element.</summary>
    /// <param name="elementName">The name of the element.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="value">The value to set.</param>
    void SetElementParameter(string elementName, string parameterName, object value);
    
    /// <summary>Gets the value of a parameter of an element.</summary>       
    /// <param name="elementName">The name of the element.</param>       
    /// <param name="parameterName">The name of the parameter.</param>       
    /// <returns>The value of the element parameter.</returns>       
    object GetElementParameter(string elementName, string parameterName);
    
    /// <summary>Gets the value of a parameter of a specific element.</summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="elementType">The type of the element.</param>
    /// <param name="elementName">The name of the element.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <returns>The value of the element parameter.</returns>
    object GetElementParameter<T>(Type elementType, string elementName, string parameterName) where T : class;
}