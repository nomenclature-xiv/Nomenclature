using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NomenclatureCommon.Domain.Exceptions;
using NomenclatureServer.Domain;
using NomenclatureServer.Services;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.InitializeSession;
using NomenclatureCommon.Domain.Network.UpdateOnlineStatus;

namespace NomenclatureServer.Hubs;

// ReSharper disable once ForCanBeConvertedToForeach

[Authorize]
public class NomenclatureHub(ConnectionService connections, DatabaseService database, ILogger<NomenclatureHub> logger) : Hub
{
    /// <summary>
    ///     Gets the account id from claims
    /// </summary>
    private string SyncCode => Context.User?.FindFirst(AuthClaimType.SyncCode)?.Value ?? throw new MissingExpectedClaimException($"{AuthClaimType.SyncCode} is not present in claims");
    
    [HubMethodName(HubMethod.InitializeSession)]
    public async Task<InitializeSessionResponse> InitializeSession(InitializeSessionRequest request)
    {
        var syncCode = SyncCode;
        if (connections.TryGetConnectedClient(syncCode) is not { } information)
            return new InitializeSessionResponse();
        
        information.CharacterName = request.CharacterName;
        information.CharacterWorld = request.CharacterWorld;

        var results = new List<PairRelationship>();
        foreach (var pair in await database.GetAllPairs(syncCode))
        {
            if (pair.GrantedByTarget is null)
            {
                results.Add(new PairRelationship(pair, PairOnlineStatus.Pending));
                continue;
            }

            if (connections.TryGetConnectedClient(pair.TargetSyncCode) is not { } target)
            {
                results.Add(new PairRelationship(pair, PairOnlineStatus.Offline));
                continue;
            }
            
            results.Add(new PairRelationship(pair, PairOnlineStatus.Online));
            
            try
            {
                var forward = new UpdateOnlineStatusRequest(syncCode, PairOnlineStatus.Online, pair.GrantedToTarget);
                await Clients.Client(target.ConnectionId).SendAsync(HubMethod.UpdateOnlineStatus, forward);
            }
            catch (Exception e)
            {
                logger.LogError("[InitializeSession] {Error}", e);
            }
        }
        
        return new InitializeSessionResponse(syncCode, results);
    }
    
    public override Task OnConnectedAsync()
    {
        connections.AddConnectedClient(SyncCode, Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        connections.RemoveConnectedClient(SyncCode);
        
        // TODO: Send a message to all online, two-way-paired friends that this client is offline now
        
        return base.OnDisconnectedAsync(exception);
    }
}