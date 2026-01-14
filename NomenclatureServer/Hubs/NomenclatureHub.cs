using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NomenclatureCommon.Domain.Exceptions;
using NomenclatureServer.Domain;
using NomenclatureServer.Services;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.InitializeSession;
using NomenclatureCommon.Domain.Network.RemoveNomenclature;
using NomenclatureCommon.Domain.Network.UpdateNomenclature;
using NomenclatureCommon.Domain.Network.UpdateOnlineStatus;

// ReSharper disable RedundantBoolCompare

namespace NomenclatureServer.Hubs;

[Authorize]
public class NomenclatureHub(ConnectionService connections, DatabaseService database, NomenclatureService nomenclatures, ILogger<NomenclatureHub> logger) : Hub
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
            return new InitializeSessionResponse(RequestErrorCode.NotAuthenticatedOrOnline, string.Empty, []);
        
        information.CharacterName = request.CharacterName;
        information.CharacterWorld = request.CharacterWorld;
        
        // TODO: Authentication of the Nomenclature values
        
        nomenclatures.Upsert(syncCode, request.Nomenclature);

        var results = new List<PairRelationship>();
        foreach (var pair in await database.GetAllPairs(syncCode))
        {
            var nomenclature = nomenclatures.TryGet(pair.TargetSyncCode);
            
            if (pair.GrantedByTarget is null)
            {
                results.Add(new PairRelationship(pair, PairOnlineStatus.Pending, nomenclature));
                continue;
            }

            if (connections.TryGetConnectedClient(pair.TargetSyncCode) is not { } target)
            {
                results.Add(new PairRelationship(pair, PairOnlineStatus.Offline, nomenclature));
                continue;
            }
            
            results.Add(new PairRelationship(pair, PairOnlineStatus.Online, nomenclature));
            
            try
            {
                var forward = new UpdateOnlineStatusForwardedRequest(syncCode, PairOnlineStatus.Online, pair.GrantedToTarget, request.Nomenclature);
                await Clients.Client(target.ConnectionId).SendAsync(HubMethod.UpdateOnlineStatus, forward);
            }
            catch (Exception e)
            {
                logger.LogError("[InitializeSession] {Error}", e);
            }
        }
        
        return new InitializeSessionResponse(RequestErrorCode.Success, syncCode, results);
    }

    [HubMethodName(HubMethod.UpdateNomenclature)]
    public async Task<UpdateNomenclatureResponse> UpdateNomenclature(UpdateNomenclatureRequest request)
    {
        var syncCode = SyncCode;
        
        // TODO: Verification that the Nomenclature is valid
        
        nomenclatures.Upsert(syncCode, request.Nomenclature);
        
        foreach (var pair in await database.GetAllPairs(syncCode))
        {
            if (pair.GrantedByTarget is null) continue;
            if (connections.TryGetConnectedClient(pair.TargetSyncCode) is not { } target) continue;
            
            try
            {
                var forward = new UpdateNomenclatureForwardedRequest(syncCode, request.Nomenclature);
                await Clients.Client(target.ConnectionId).SendAsync(HubMethod.UpdateNomenclature, forward);
            }
            catch (Exception e)
            {
                logger.LogError("[UpdateNomenclature] {Error}", e);
            }
        }
        
        return new UpdateNomenclatureResponse(RequestErrorCode.Success);
    }
    
    [HubMethodName(HubMethod.RemoveNomenclature)]
    public async Task<RemoveNomenclatureResponse> RemoveNomenclature(RemoveNomenclatureRequest request)
    {
        var syncCode = SyncCode;
        
        if (nomenclatures.Remove(syncCode) is false)
            return new RemoveNomenclatureResponse(RequestErrorCode.Success);
        
        foreach (var pair in await database.GetAllPairs(syncCode))
        {
            if (pair.GrantedByTarget is null) continue;
            if (connections.TryGetConnectedClient(pair.TargetSyncCode) is not { } target) continue;
            
            try
            {
                var forward = new RemoveNomenclatureForwardedRequest(syncCode);
                await Clients.Client(target.ConnectionId).SendAsync(HubMethod.RemoveNomenclature, forward);
            }
            catch (Exception e)
            {
                logger.LogError("[RemoveNomenclature] {Error}", e);
            }
        }
        
        return new RemoveNomenclatureResponse(RequestErrorCode.Success);
    }
    
    public override Task OnConnectedAsync()
    {
        connections.AddConnectedClient(SyncCode, Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var syncCode = SyncCode;
        connections.RemoveConnectedClient(syncCode);

        foreach (var pair in await database.GetAllPairs(syncCode))
        {
            if (pair.GrantedByTarget is null) continue;
            if (connections.TryGetConnectedClient(pair.TargetSyncCode) is not { } target) continue;
            
            try
            {
                var forward = new UpdateOnlineStatusForwardedRequest(syncCode, PairOnlineStatus.Offline, null, null);
                await Clients.Client(target.ConnectionId).SendAsync(HubMethod.UpdateOnlineStatus, forward);
            }
            catch (Exception e)
            {
                logger.LogError("[OnDisconnectedAsync] {Error}", e);
            }
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}