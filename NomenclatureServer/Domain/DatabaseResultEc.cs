namespace NomenclatureServer.Domain;

/// <summary>
///     Represents the result of a database command
/// </summary>
public enum DatabaseResultEc
{
    /// <summary>
    ///     Default value, never should be encountered
    /// </summary>
    Uninitialized,
    
    /// <summary>
    ///     No operation was performed (such as no rows updated)
    /// </summary>
    NoOp,
    
    /// <summary>
    ///     No such account exists
    /// </summary>
    NoSuchSyncCode,
    
    /// <summary>
    ///     The friend code already exists
    /// </summary>
    SyncCodeAlreadyExists,
    
    /// <summary>
    ///     Already paired with a target
    /// </summary>
    AlreadyPaired,
    
    /// <summary>
    ///     Successfully paired with a user, but awaiting them to pair you back
    /// </summary>
    Pending,
    
    /// <summary>
    ///     Unknown issue occurred
    /// </summary>
    Unknown,
    
    /// <summary>
    ///     Action was successful
    /// </summary>
    Success
}