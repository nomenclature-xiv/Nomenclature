using System;

namespace Nomenclature.Types.Exceptions;

/// <summary>
///     Exception thrown when a client sends an invalid token when logging into the server
/// </summary>
public class InvalidSecretException : Exception
{
    /// <summary>
    ///     <inheritdoc cref="InvalidSecretException"/>
    /// </summary>
    public InvalidSecretException()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="InvalidSecretException"/>
    /// </summary>
    public InvalidSecretException(string message) : base(message)
    {
    }
    
    /// <summary>
    ///     <inheritdoc cref="InvalidSecretException"/>
    /// </summary>
    public InvalidSecretException(string message, Exception inner) : base(message, inner)
    {
    }
}