using System;

namespace NomenclatureClient.Types.Exceptions;

/// <summary>
///     Exception thrown when a client attempts to connect to the server without having a secret assigned to their local chartacter
/// </summary>
public class NoSecretForLocalCharacterException : Exception
{
    /// <summary>
    ///     <inheritdoc cref="NoSecretForLocalCharacterException"/>
    /// </summary>
    public NoSecretForLocalCharacterException()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="NoSecretForLocalCharacterException"/>
    /// </summary>
    public NoSecretForLocalCharacterException(string message) : base(message)
    {
    }

    /// <summary>
    ///     <inheritdoc cref="NoSecretForLocalCharacterException"/>
    /// </summary>
    public NoSecretForLocalCharacterException(string message, Exception inner) : base(message, inner)
    {
    }
}