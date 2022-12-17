namespace Asou.Core;

public enum Instructions : byte
{
    None = 0,

    /// <summary>
    ///     Components of system: processes, elements, connections, etc.
    ///     Arguments:
    ///     * component type: string
    ///     * component name: string
    /// </summary>
    CreateComponent = 0x10,

    /// <summary>
    ///     Arguments:
    ///     * object type name: string
    ///     * object name: string
    /// </summary>
    CreateObject = 0x11,

    /// <summary>
    ///     Arguments:
    ///     * parameter name: string
    /// </summary>
    LetParameter = 0x30,

    /// <summary>
    ///     Arguments:
    ///     * parameter name: string
    /// </summary>
    DeleteParameter = 0x31,

    /// <summary>
    ///     Arguments:
    ///     * parameter name: string
    ///     * argument type: string
    ///     * argument value: any
    /// </summary>
    SetParameter = 0x32,

    /// <summary>
    ///     Arguments:
    ///     * element name: string
    /// </summary>
    CallProcedure = 0x50,

    /// <summary>
    ///     Arguments:
    ///     * element name: string
    /// </summary>
    ExecuteElement = 0x51,

    /// <summary>
    ///     Arguments:
    ///     * element name: string
    /// </summary>
    AfterExecuteElement = 0x52,

    /// <summary>
    ///     Arguments:
    ///     * element name: string
    /// </summary>
    ConfigureAwaiter = 0x53,

    /// <summary>
    ///     Arguments:
    ///     * element name: string
    /// </summary>
    ValidateEvent = 0x54,

    /// <summary>
    ///     Arguments:
    ///     * element name: string
    /// </summary>
    AfterAwaiter = 0x55,

    /// <summary>
    ///     Arguments:
    ///     * element name: string
    ///     * eventBody: unknown? data container
    /// </summary>
    DispatchEvent = 0x56,

    /// <summary>
    ///     Arguments:
    ///     * name: string
    ///     * length: int
    ///     * body: bytes
    /// </summary>
    DeclareScript = 0x61,

    /// <summary>
    ///     Arguments:
    ///     * name: string
    /// </summary>
    CallScript = 0x62,
    Return = 0x63,

    /// <summary>
    ///     Arguments:
    ///     * condition: unknown?
    ///     * then script name: string
    ///     * else script name: string
    /// </summary>
    IfStatement = 0x64
}