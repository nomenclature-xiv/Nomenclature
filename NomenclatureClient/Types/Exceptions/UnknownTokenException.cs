using System;

namespace NomenclatureClient.Types.Exceptions;

/// <summary>
///     Exception thrown when an unknown response code is provided
/// </summary>
public class UnknownTokenException : Exception
{
    /// <summary>
    ///     <inheritdoc cref="UnknownTokenException"/>
    /// </summary>
    public UnknownTokenException()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="UnknownTokenException"/>
    /// </summary>
    public UnknownTokenException(string message) : base(message)
    {
    }
    
    /// <summary>
    ///     <inheritdoc cref="UnknownTokenException"/>
    /// </summary>
    public UnknownTokenException(string message, Exception inner) : base(message, inner)
    {
    }
}