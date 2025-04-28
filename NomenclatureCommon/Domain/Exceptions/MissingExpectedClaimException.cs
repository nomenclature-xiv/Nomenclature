namespace NomenclatureCommon.Domain.Exceptions;

/// <summary>
///     Exception thrown when a claim required to process a request is missing
/// </summary>
public class MissingExpectedClaimException : Exception
{
    /// <summary>
    ///     <inheritdoc cref="MissingExpectedClaimException"/>
    /// </summary>
    public MissingExpectedClaimException(string message) : base(message) { }
    
    /// <summary>
    ///     <inheritdoc cref="MissingExpectedClaimException"/>
    /// </summary>
    public MissingExpectedClaimException(string message, Exception inner) : base(message, inner) { }
}