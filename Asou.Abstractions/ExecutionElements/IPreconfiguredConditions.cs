namespace Asou.Abstractions.ExecutionElements;

/// <summary>Interface represents <see cref="BaseElement" /> with preconfigured conditions for connections.</summary>
public interface IPreconfiguredConditions
{
    /// <summary>Checks a condition and get bool representing connection pass.</summary>
    /// <param name="name">The name of the condition to check.</param>
    bool CheckCondition(string name);
}