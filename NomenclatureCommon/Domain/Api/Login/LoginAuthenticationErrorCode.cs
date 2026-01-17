namespace NomenclatureCommon.Domain.Api.Login;

public enum LoginAuthenticationErrorCode
{
    /// <summary>
    ///     Something was not populated in SignalR transport
    /// </summary>
    Uninitialized,
    
    /// <summary>
    ///     Successfully authenticated
    /// </summary>
    Success,
    
    /// <summary>
    ///     Client attempted to connect with an incompatible version of the plugin
    /// </summary>
    VersionMismatch,
    
    /// <summary>
    ///     The provide secret was not found
    /// </summary>
    UnknownSecret,
    
    /// <summary>
    ///     An unknown failure
    /// </summary>
    Unknown
}