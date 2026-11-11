using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NetStone.Definitions.Model.CWLS;
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
using NomenclatureCommon.Domain.Network.RemoveZone;
using NomenclatureCommon.Domain.Network.Responses;
using NomenclatureCommon.Domain.Network.UpdateNomenclature;
using NomenclatureCommon.Domain.Network.UpdateOnlineStatus;
using NomenclatureCommon.Domain.Network.UpdateZone;
using NomenclatureCommon.Domain.Network.Zones;
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
            return new InitializeSessionResponse(RequestErrorCode.NotAuthenticatedOrOnline, string.Empty, [], null);

        information.CharacterName = request.CharacterName;
        information.CharacterWorld = request.CharacterWorld;

        if (request.NomenclatureDto is not null)
        {
            if (Validator.TryValidateNomenclature(request.NomenclatureDto) is false)
                return new InitializeSessionResponse(RequestErrorCode.InvalidNomenclature, syncCode, [], null);

            nomenclatures.Upsert(syncCode, request.NomenclatureDto);
        }

        var results = new List<PairDto>();
        foreach (var pair in await database.GetAllPairs(syncCode))
        {
            if (pair.IsOneWay)
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
                logger.LogWarning("A connected client {SyncCode} did not have a corresponding Nomenclature", pair.SyncCode);
                results.Add(new OnlinePairDto(pair.SyncCode, null, target.CharacterName, target.CharacterWorld));
            }
            else
            {
                results.Add(new OnlinePairDto(pair.SyncCode, nomenclature, target.CharacterName,
                    target.CharacterWorld));
            }

            try
            {
                var forward = new UpdateOnlineStatusForwardedRequest(new OnlinePairDto(syncCode, request.NomenclatureDto, information.CharacterName, information.CharacterWorld));
                await Clients.Client(target.ConnectionId).SendAsync(HubMethod.UpdateOnlineStatus, forward);
            }
            catch (Exception e)
            {
                logger.LogError("[InitializeSession] {Error}", e);
            }
        }

        //handling zonesync if we have it on
        var zoneNomenclatures = await DoUpdateZone(request.ZoneDto, request.NomenclatureDto);

        return new InitializeSessionResponse(RequestErrorCode.Success, syncCode, results, zoneNomenclatures);
    }

    [HubMethodName(HubMethod.AddPair)]
    public async Task<PairResponse<PairDto>> AddPair(AddPairRequest request)
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
        if (code is not (PairResponseErrorCode.Success or PairResponseErrorCode.PairPending))
            return new PairResponse<PairDto>(code, null);
        
        if(code is PairResponseErrorCode.PairPending)
            return new PairResponse<PairDto>(code, new PendingPairDto(request.TargetPairCode));

        // If they're not online, exit early
        if (connections.TryGetConnectedClient(request.TargetPairCode) is not { } target)
            return new PairResponse<PairDto>(code, new OfflinePairDto(request.TargetPairCode));
        
        // If they're online, send our nomenclature to them
        try
        {
            if (connections.TryGetConnectedClient(senderSyncCode) is not { } sender)
            {
                // This shouldn't happen ever...
                return new PairResponse<PairDto>(code, null);
            }

            if (nomenclatures.TryGet(senderSyncCode) is not { } senderNomenclature)
                senderNomenclature = null;
            
            var online = new OnlinePairDto(senderSyncCode, senderNomenclature, sender.CharacterName, sender.CharacterWorld);
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
        
        return new PairResponse<PairDto>(code, new OnlinePairDto(request.TargetPairCode, targetNomenclature, target.CharacterName, target.CharacterWorld));
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

    [HubMethodName(HubMethod.UpdateNomenclature)]
    public async Task<UpdateNomenclatureResponse> UpdateNomenclature(UpdateNomenclatureRequest request)
    {
        var syncCode = SyncCode;
        if (Validator.TryValidateNomenclature(request.NomenclatureDto) is false)
            return new UpdateNomenclatureResponse(RequestErrorCode.InvalidNomenclature);

        nomenclatures.Upsert(syncCode, request.NomenclatureDto);

        var pairs = await database.GetAllPairs(syncCode);
        foreach (var pair in pairs)
        {
            if (pair.IsOneWay) continue;
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
        
        if (connections.ConnectedClients.TryGetValue(SyncCode, out var info))
        {
            foreach (var client in connections.ConnectedClients)
            {
                if (pairs.Any(pair => pair.SyncCode == client.Key && !pair.IsOneWay))
                    continue;
                if (client.Value.Zone != info.Zone)
                    continue;
                var req = new UpdateNomenclatureZoneForwardedRequest(info.CharacterName, info.CharacterWorld, request.NomenclatureDto);
                await Clients.Client(client.Value.ConnectionId).SendAsync(HubMethod.UpdateZone, req);
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

        var pairs = await database.GetAllPairs(syncCode);
        foreach (var pair in pairs)
        {
            if (pair.IsOneWay) continue;
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
        
        if (connections.ConnectedClients.TryGetValue(SyncCode, out var info))
        {
            foreach (var client in connections.ConnectedClients)
            {
                if (pairs.Any(pair => pair.SyncCode == client.Key && !pair.IsOneWay))
                    continue;
                if (client.Value.Zone != info.Zone)
                    continue;
                var req = new RemoveNomenclatureZoneForwardedRequest(info.CharacterName, info.CharacterWorld);
                await Clients.Client(client.Value.ConnectionId).SendAsync(HubMethod.RemoveZone, req);
            }
        }

        return new RemoveNomenclatureResponse(RequestErrorCode.Success);
    }

    [HubMethodName(HubMethod.UpdateZone)]
    public async Task<UpdateZoneResponse> UpdateZone(UpdateZoneRequest request)
    {
        var nomenclature = nomenclatures.TryGet(SyncCode);
        var zoneNomenclatures = await DoUpdateZone(request.Zone, nomenclature);
        return new UpdateZoneResponse(zoneNomenclatures!);
    }

    [HubMethodName(HubMethod.RemoveZone)]
    public async Task<RemoveZoneResponse> RemoveZone(RemoveZoneRequest request)
    {
        var pairs = await database.GetAllPairs(SyncCode);
        if (connections.ConnectedClients.TryGetValue(SyncCode, out var info))
        {
            foreach (var client in connections.ConnectedClients)
            {
                if (pairs.Any(pair => pair.SyncCode == client.Key && !pair.IsOneWay))
                    continue;
                if (client.Value.Zone != info.Zone)
                    continue;
                var req = new RemoveNomenclatureZoneForwardedRequest(info.CharacterName, info.CharacterWorld);
                await Clients.Client(client.Value.ConnectionId).SendAsync(HubMethod.RemoveZone, req);
            }
        }
        return new  RemoveZoneResponse(RequestErrorCode.Success);
    }

    private async Task<List<ZonemenclatureDto>?> DoUpdateZone(ZoneDto? zoneDto, NomenclatureDto? nomenclatureDto)
    {
        List<ZonemenclatureDto>? zoneNomenclatures = null;
        if (zoneDto is not null)
        {
            if (!connections.ConnectedClients.TryGetValue(SyncCode, out var info))
                return null;
            zoneNomenclatures = new ();
            var zone = ZoneInformation.FromDto(zoneDto);
            connections.UpdateClientZone(SyncCode, zone);
            var pairs = await database.GetAllPairs(SyncCode);
            foreach (var client in connections.ConnectedClients)
            {
                if (pairs.Any(pair => pair.SyncCode == client.Key && !pair.IsOneWay))
                    continue;
                if (client.Value.Zone != zone)
                    continue;
                var req = new UpdateNomenclatureZoneForwardedRequest(info.CharacterName, info.CharacterWorld, nomenclatureDto);
                await Clients.Client(client.Value.ConnectionId).SendAsync(HubMethod.UpdateZone, req);
                if (nomenclatures.TryGet(client.Key) is { } nomenclature)
                {
                    zoneNomenclatures.Add(new ZonemenclatureDto(client.Value.CharacterName,
                        client.Value.CharacterWorld, nomenclature));
                }
            }
        }
        return zoneNomenclatures;
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

        var pairs =  await database.GetAllPairs(syncCode);
        foreach (var pair in pairs)
        {
            if (pair.IsOneWay) continue;
            if (connections.TryGetConnectedClient(pair.SyncCode) is not { } target) continue;

            try
            {
                var forward = new UpdateOnlineStatusForwardedRequest(new OfflinePairDto(SyncCode));
                await Clients.Client(target.ConnectionId).SendAsync(HubMethod.UpdateOnlineStatus, forward);
            }
            catch (Exception e)
            {
                logger.LogError("[OnDisconnectedAsync] {Error}", e);
            }
        }
        
        if (connections.ConnectedClients.TryGetValue(SyncCode, out var info))
        {
            foreach (var client in connections.ConnectedClients)
            {
                if (pairs.Any(pair => pair.SyncCode == client.Key && !pair.IsOneWay))
                    continue;
                if (client.Value.Zone != info.Zone)
                    continue;
                var req = new RemoveNomenclatureZoneForwardedRequest(info.CharacterName, info.CharacterWorld);
                await Clients.Client(client.Value.ConnectionId).SendAsync(HubMethod.UpdateZone, req);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}