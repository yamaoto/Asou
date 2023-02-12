using Asou.Abstractions.Events;

namespace Asou.Abstractions.Process;

/// <summary>Processes commands from the process state machine.</summary>
/// <remarks>This interface is by process engines to communicate with process state machine.</remarks>
public interface IProcessMachineCommands : IProcessMachine
{
    /// <summary>Creates a component.</summary>
    /// <param name="componentId">Id of component to create.</param>
    /// <param name="componentType">The component type.</param>
    void CreateComponent(Guid componentId, Type componentType);

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

    /// <summary>Executes an element in the current process.</summary>
    /// <param name="elementId">id of the element to execute.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the operation.</returns>
    Task ExecuteElementAsync(Guid elementId, CancellationToken cancellationToken = default);

    /// <summary>Called after an element is executed.</summary>
    /// <param name="elementId">id of the element.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the operation.</returns>
    Task AfterExecutionElementAsync(Guid elementId, CancellationToken cancellationToken = default);

    /// <summary>Configures the continuous operation with asynchronous resume for the specified element.</summary>
    /// <param name="elementId">Id the element.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the operation.</returns>
    Task<IEnumerable<EventSubscription>> ConfigureAsynchronousResumeAsync(Guid elementId,
        CancellationToken cancellationToken = default);

    /// <summary>Sets the value of a parameter on an element.</summary>
    /// <param name="elementId">id of the element.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="value">The value to set.</param>
    void SetElementParameter(Guid elementId, string parameterName, object value);

    /// <summary>Gets the value of a parameter of an element.</summary>
    /// <param name="elementId">id of the element.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <returns>The value of the element parameter.</returns>
    object GetElementParameter(Guid elementId, string parameterName);

    /// <summary>Gets the value of a parameter of a specific element.</summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="elementType">The type of the element.</param>
    /// <param name="elementId">id of the element.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <returns>The value of the element parameter.</returns>
    object GetElementParameter<T>(Type elementType, Guid elementId, string parameterName) where T : class;

    /// <summary>
    ///     Validate asynchronous resume event applicability on a specific element
    /// </summary>
    /// <param name="elementId">id of the element.</param>
    /// <param name="eventRepresentation">Event representation</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the operation.</returns>
    /// <returns></returns>
    Task<bool> ValidateSubscriptionEventAsync(Guid elementId, EventRepresentation eventRepresentation,
        CancellationToken cancellationToken = default);
}
