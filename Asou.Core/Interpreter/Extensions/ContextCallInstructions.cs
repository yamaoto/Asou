namespace Asou.Core.Interpreter.Extensions;

public enum ContextCallInstructions : byte
{
    /// <summary>
    ///     Arguments:
    ///     * argument type: string
    ///     * argument value: any
    ///     * parameter name: string
    /// </summary>
    ConstLiteral,

    /// <summary>
    ///     Arguments:
    ///     * path: string
    ///     * parameter name: string
    /// </summary>
    GetConfig,

    /// <summary>
    ///     Arguments:
    ///     * name: string
    ///     ?
    ///     * parameter name: string
    /// </summary>
    CallFunc,

    /// <summary>
    ///     Arguments:
    ///     * ?
    ///     * parameter name: string
    /// </summary>
    ObjectReflection,

    /// <summary>
    ///     Arguments:
    ///     * name: string
    ///     * parameter name: string
    /// </summary>
    GetVariableValue,

    /// <summary>
    ///     Arguments:
    ///     * name: string
    ///     * parameter name: string
    /// </summary>
    GetProcessParameter,

    /// <summary>
    ///     Arguments:
    ///     * element name: string
    ///     * parameter name: string
    ///     * parameter name: string
    /// </summary>
    GetElementParameter,

    /// <summary>
    ///     Arguments:
    ///     * name: string
    ///     * parameter name: string
    /// </summary>
    GetElement,

    /// <summary>
    ///     Arguments:
    ///     * parameter name: string
    ///     * parameter name: string
    /// </summary>
    GetCurrentElementParameter,

    /// <summary>
    ///     Arguments:
    ///     * ?
    ///     * parameter name: string
    /// </summary>
    CurrentObjectReflection
}