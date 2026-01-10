namespace NomenclatureServer.Domain;

/// <summary>
///     Information about a connected client's current session
/// </summary>
public class SessionInformation(string connectionId)
{
    /// <summary>
    ///     The SignalR connection id granted to this client
    /// </summary>
    public readonly string ConnectionId = connectionId;
    
    /// <summary>
    ///     The character this account is currently logged in as
    /// </summary>
    public string CharacterName = string.Empty;
    
    /// <summary>
    ///     The world of the character this account is currently logged in as
    /// </summary>
    public string CharacterWorld = string.Empty;
    
    /// <summary>
    ///     The last time this user submitted a command
    /// </summary>
    public DateTime LastAction = DateTime.Now;
}