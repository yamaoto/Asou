using Asou.Abstractions.ExecutionElements;

namespace Asou.Abstractions;

/// <summary>A process state machine.</summary>
public interface IProcessMachine
{
    /// <summary>Gets or init the name of the process.</summary>
    /// <value>The name of the process.</value>
    string Name { get; init; }

    /// <summary>Gets the components of the process.</summary>
    /// <returns>The components of the process.</returns>
    IReadOnlyDictionary<Guid, BaseElement> Components { get; }

    /// <summary>Gets the parameters of the process.</summary>
    /// <returns>The parameters of the process.</returns>
    ProcessParameters Parameters { get; }
}
