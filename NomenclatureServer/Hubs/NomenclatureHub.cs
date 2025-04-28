using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Api;
using NomenclatureCommon.Domain.Api.Base;
using NomenclatureCommon.Domain.Api.Server;
using NomenclatureServer.Domain;
using NomenclatureServer.Services;

namespace NomenclatureServer.Hubs;

[Authorize]
public class NomenclatureHub(NomenclatureService nomenclatureService, ILogger<NomenclatureHub> logger) : Hub
{
    [HubMethodName(ApiMethods.ClearName)]
    public Response ClearName(ClearNameRequest request)
    {
        logger.LogInformation("{Request}", request);
        Character self = GetCharacterFromClaims();
        nomenclatureService.Nomenclatures.Remove(self);
        Clients.Group(self.ToString()).SendAsync(ApiMethods.RemoveNomenclature, self);
        return new Response { Success = true };
    }

    [HubMethodName(ApiMethods.SetName)]
    public Response SetName(SetNameRequest request)
    {
        logger.LogInformation("{Request}", request);
        var character = GetCharacterFromClaims();
        Nomenclature nomenclature;
        if (nomenclatureService.Nomenclatures.TryGetValue(character, out var existingNomenclature))
        {
            var name = request.Nomenclature.Name ?? existingNomenclature.Name;
            var world = request.Nomenclature.World ?? existingNomenclature.World;
            nomenclature = new Nomenclature(name, world);
            nomenclatureService.Nomenclatures[character] = nomenclature;
        }
        else
        {
            nomenclatureService.Nomenclatures[character] = request.Nomenclature;
            nomenclature = request.Nomenclature;
        }
        Clients.Group(character.ToString()).SendAsync(ApiMethods.UpdateNomenclature, character, nomenclature);

        return new Response { Success = true };
    }

    [HubMethodName(ApiMethods.QueryChangedNames)]
    public Response QueryChangedNames(QueryChangedNamesRequest request)
    {
        logger.LogInformation("{Request}", request);

        var toremove = request.Remove.AsSpan();
        foreach (var character in toremove)
        {
            Groups.RemoveFromGroupAsync(Context.ConnectionId, character.ToString());
            Clients.Client(Context.ConnectionId).SendAsync(ApiMethods.RemoveNomenclature, character.ToString());
        }
        var toadd = request.Add.AsSpan();
        foreach (var character in toadd)
        {
            Groups.AddToGroupAsync(Context.ConnectionId, character.ToString());
            nomenclatureService.Nomenclatures.TryGetValue(character, out var nomenclature);
            if(nomenclature is not null)
                Clients.Group(character.ToString()).SendAsync(ApiMethods.UpdateNomenclature, character, nomenclature);
        }
        
        return new Response { Success = true };
    }
    
    private Character GetCharacterFromClaims()
    {
        var name = Context.User?.FindFirst(AuthClaimType.CharacterName)?.Value ?? throw new Exception("CharacterName is not present in claims");
        var world = Context.User?.FindFirst(AuthClaimType.WorldName)?.Value ?? throw new Exception("WorldName is not present in claims");
        return new Character(name, world);
    }
}