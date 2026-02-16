namespace NomenclatureCommon.Domain.Network.Responses;

public enum PairResponseErrorCode
{
    /// <summary>
    ///     Default value, should never be used, and indicates serialization failure
    /// </summary>
    Uninitialized,
    
    /// <summary>
    ///     The pair code you are trying to interact with doesn't exist
    /// </summary>
    NoSuchPairCode,
    
    /// <summary>
    ///     You have successfully paired with someone, but the pair is one-sided and must have them pair you back
    /// </summary>
    PairPending,
    
    /// <summary>
    ///     You probably have a good idea what this indicates
    /// </summary>
    Success,
    
    /// <summary>
    ///     Something unusual happened that was not expected, consider this a catch-all failure state
    /// </summary>
    Unknown
}