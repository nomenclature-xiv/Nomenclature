using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Exceptions;
using NomenclatureServer.Domain;
using NomenclatureServer.Services;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.InitializeSession;
using NomenclatureCommon.Domain.Network.Pairs;
using NomenclatureCommon.Domain.Network.Pairs.AddPair;
using NomenclatureCommon.Domain.Network.Pairs.PausePair;
using NomenclatureCommon.Domain.Network.Pairs.RemovePair;
using NomenclatureCommon.Domain.Network.RemoveNomenclature;
using NomenclatureCommon.Domain.Network.Responses;
using NomenclatureCommon.Domain.Network.UpdateNomenclature;
using NomenclatureCommon.Domain.Network.UpdateOnlineStatus;
using NomenclatureServer.Utilities;

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

        if (Validator.TryValidateNomenclature(request.NomenclatureDto) is false)
            return new InitializeSessionResponse(RequestErrorCode.InvalidNomenclature, syncCode, []);

        nomenclatures.Upsert(syncCode, request.NomenclatureDto);

        var results = new List<PairDto>();
        foreach (var pair in await database.GetAllPairs(syncCode))
        {
            if (pair.IsOneWay())
            {
                results.Add(new PendingPairDto(pair.SyncCode));
                continue;
            }

            if (connections.TryGetConnectedClient(pair.SyncCode) is not { } target)
            {
                results.Add(new OfflinePairDto(pair.SyncCode));
                continue;
            }

            if (nomenclatures.TryGet(pair.SyncCode) is not { } nomenclature)
            {
                logger.LogWarning("A connected client {SyncCode} did not have a corresponding Nomenclature, did they log out?", pair.SyncCode);
                results.Add(new OfflinePairDto(pair.SyncCode));
                continue;
            }

            results.Add(new OnlinePairDto(pair.SyncCode, pair.LeftSidePaused, pair.RightSidePaused ?? false, nomenclature, target.CharacterName, target.CharacterWorld));

            try
            {
                var forward = new UpdateOnlineStatusForwardedRequest(new OnlinePairDto(syncCode, pair.LeftSidePaused, pair.RightSidePaused ?? false, request.NomenclatureDto, information.CharacterName, information.CharacterWorld));
                await Clients.Client(target.ConnectionId).SendAsync(HubMethod.UpdateOnlineStatus, forward);
            }
            catch (Exception e)
            {
                logger.LogError("[InitializeSession] {Error}", e);
            }
        }

        return new InitializeSessionResponse(RequestErrorCode.Success, syncCode, results);
    }

    [HubMethodName(HubMethod.AddPair)]
    public async Task<PairResponse<NomenclatureDto>?> AddPair(AddPairRequest request)
    {
        var senderSyncCode = SyncCode;
        var result = await database.CreatePair(senderSyncCode, request.TargetPairCode);
        var code = result switch
        {
            DatabaseResultEc.Success or DatabaseResultEc.AlreadyPaired => PairResponseErrorCode.Success,
            DatabaseResultEc.Pending => PairResponseErrorCode.PairPending,
            DatabaseResultEc.NoSuchSyncCode => PairResponseErrorCode.NoSuchPairCode,
            _ => PairResponseErrorCode.Unknown
        };

        // Exit early if not in a success state
        if (code is not PairResponseErrorCode.Success and PairResponseErrorCode.PairPending)
            return new PairResponse<NomenclatureDto>(code, null);
        
        // If they're not online, exit early
        if (connections.TryGetConnectedClient(request.TargetPairCode) is not { } target)
            return new PairResponse<NomenclatureDto>(code, null);
        
        // If they're online, send our nomenclature to them
        try
        {
            if (connections.TryGetConnectedClient(senderSyncCode) is not { } sender)
            {
                // This shouldn't happen ever...
                return new PairResponse<NomenclatureDto>(code, null);
            }

            if (nomenclatures.TryGet(senderSyncCode) is not { } senderNomenclature)
                senderNomenclature = null;
            
            var online = new OnlinePairDto(senderSyncCode, false, false, senderNomenclature, sender.CharacterName, sender.CharacterWorld);
            var sync = new UpdateOnlineStatusForwardedRequest(online);
            await Clients.Clients(target.ConnectionId).SendAsync(HubMethod.UpdateOnlineStatus, sync);
        }
        catch (Exception e)
        {
            logger.LogError("[AddPair] Error while sending updated status, {Error}", e);
        }

        // Grab their nomenclature if they have one
        if (nomenclatures.TryGet(request.TargetPairCode) is not { } targetNomenclature)
            targetNomenclature = null;
        
        return new PairResponse<NomenclatureDto>(code, targetNomenclature);
    }

    [HubMethodName(HubMethod.RemovePair)]
    public async Task<PairResponse> RemovePair(RemovePairRequest request)
    {
        var result = await database.RemovePair(SyncCode, request.TargetPairCode);
        var code = result switch
        {
            DatabaseResultEc.Success or DatabaseResultEc.NoOp => PairResponseErrorCode.Success,
            _ => PairResponseErrorCode.Unknown
        };

        return new PairResponse(code);
    }

    [HubMethodName(HubMethod.PausePair)]
    public async Task<PairResponse> PausePair(PausePairRequest request)
    {
        var result = await database.PausePair(SyncCode, request.TargetPairCode, request.Pause);
        var code = result switch
        {
            DatabaseResultEc.Success or DatabaseResultEc.NoOp => PairResponseErrorCode.Success,
            _ => PairResponseErrorCode.Unknown
        };

        return new PairResponse(code);
    }

    [HubMethodName(HubMethod.UpdateNomenclature)]
    public async Task<UpdateNomenclatureResponse> UpdateNomenclature(UpdateNomenclatureRequest request)
    {
        var syncCode = SyncCode;
        if (Validator.TryValidateNomenclature(request.NomenclatureDto) is false)
            return new UpdateNomenclatureResponse(RequestErrorCode.InvalidNomenclature);

        nomenclatures.Upsert(syncCode, request.NomenclatureDto);

        foreach (var pair in await database.GetAllPairs(syncCode))
        {
            if (pair.IsOneWay()) continue;
            if (connections.TryGetConnectedClient(pair.SyncCode) is not { } target) continue;

            try
            {
                var forward = new UpdateNomenclatureForwardedRequest(syncCode, request.NomenclatureDto);
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
            if (pair.IsOneWay()) continue;
            if (connections.TryGetConnectedClient(pair.SyncCode) is not { } target) continue;

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
            if (pair.IsOneWay()) continue;
            if (connections.TryGetConnectedClient(pair.SyncCode) is not { } target) continue;

            try
            {
                var forward = new UpdateOnlineStatusForwardedRequest(new OfflinePairDto(pair.SyncCode));
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