using System.Collections.Concurrent;
using NomenclatureServer.Domain;

namespace NomenclatureServer.Services;

/// <summary>
///     Provides access to a list of connected clients and information about them
/// </summary>
public class ConnectionService
{
    /// <summary>
    ///     Thread safe dictionary containing a map of SyncCode -> SessionInformation
    /// </summary>
    private readonly ConcurrentDictionary<string, SessionInformation> _connectedClients = [];

    /// <summary>
    ///     Add a new connected client to our list
    /// </summary>
    public void AddConnectedClient(string syncCode, string connectionId) => _connectedClients[syncCode] = new SessionInformation(connectionId);
    
    /// <summary>
    ///     Try to remove a connected client from the list
    /// </summary>
    public void RemoveConnectedClient(string syncCode) => _connectedClients.TryRemove(syncCode, out _);
    
    /// <summary>
    ///     Try to get a connected client by account id
    /// </summary>
    public SessionInformation? TryGetConnectedClient(string syncCode) => _connectedClients.TryGetValue(syncCode, out var information) ? information : null;
}

