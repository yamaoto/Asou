namespace Asou.ByteCodeEngine;

public enum Instructions : byte
{
    None = 0,

    Nope = 1,

    /// <summary>
    ///     Components of system: processes, elements, connections, etc.
    ///     Arguments:
    ///     * component type: string
    ///     * component name: string
    /// </summary>
    CreateComponent,

    /// <summary>
    ///     Arguments:
    ///     * object type name: string
    ///     * object name: string
    /// </summary>
    CreateObject,

    /// <summary>
    ///     Arguments:
    ///     * parameter name: string
    /// </summary>
    LetParameter,

    /// <summary>
    ///     Arguments:
    ///     * parameter name: string
    /// </summary>
    DeleteParameter,

    /// <summary>
    ///     Arguments:
    ///     * parameter name: string
    ///     * argument type: string
    ///     * argument value: any
    /// </summary>
    SetParameter,

    /// <summary>
    ///     Arguments:
    ///     * element name: string
    /// </summary>
    CallProcedure,

    /// <summary>
    ///     Arguments:
    ///     * element name: string
    /// </summary>
    ExecuteElement,

    /// <summary>
    ///     Arguments:
    ///     * element name: string
    /// </summary>
    AfterExecuteElement,

    /// <summary>
    ///     Arguments:
    ///     * element name: string
    /// </summary>
    ConfigureAwaiter,

    /// <summary>
    ///     Arguments:
    ///     * element name: string
    /// </summary>
    ValidateEvent,

    /// <summary>
    ///     Arguments:
    ///     * element name: string
    /// </summary>
    AfterAwaiter,

    /// <summary>
    ///     Arguments:
    ///     * element name: string
    ///     * eventBody: unknown? data container
    /// </summary>
    DispatchEvent,

    /// <summary>
    ///     Arguments:
    ///     * name: string
    ///     * length: int
    ///     * body: bytes
    /// </summary>
    DeclareScript,

    /// <summary>
    ///     Arguments:
    ///     * name: string
    /// </summary>
    CallScript,

    Return,

    /// <summary>
    ///     Arguments:
    ///     * condition: unknown?
    ///     * then command
    ///     * else command
    /// </summary>
    IfStatement,

    /// <summary>
    ///     Arguments:
    ///     * instruction: 1byte
    /// </summary>
    Extension
}