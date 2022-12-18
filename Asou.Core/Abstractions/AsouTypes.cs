namespace Asou.Core.Abstractions;

public enum AsouTypes : byte
{
    /// <summary>
    ///     Do not use this type. It used only by system
    /// </summary>
    UnSet = 0,

    /// <summary>
    ///     Boolean
    /// </summary>
    Boolean = 1,

    /// <summary>
    ///     Integer
    /// </summary>
    Integer = 2,

    /// <summary>
    ///     Float
    /// </summary>
    Float = 3,

    /// <summary>
    ///     Decimal
    /// </summary>
    Decimal = 4,

    /// <summary>
    ///     String
    /// </summary>
    String = 5,

    /// <summary>
    ///     DateTime
    /// </summary>
    DateTime = 6,

    /// <summary>
    ///     Guid / uuid / unique-identifier
    /// </summary>
    Guid = 7,

    /// <summary>
    ///     Object type
    /// </summary>
    Object = 8,

    /// <summary>
    ///     ObjectLink
    /// </summary>
    ObjectLink = 9,

    /// <summary>
    ///     NullObject
    /// </summary>
    NullObject = 255
}