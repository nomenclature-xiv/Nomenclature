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
        nomenclatureService.Nomenclatures.Remove(GetCharacterFromClaims());
        return new Response { Success = true };
    }

    [HubMethodName(ApiMethods.SetName)]
    public Response SetName(SetNameRequest request)
    {
        logger.LogInformation("{Request}", request);
        nomenclatureService.Nomenclatures[GetCharacterFromClaims()] = request.Nomenclature;
        return new Response { Success = true };
    }

    [HubMethodName(ApiMethods.QueryChangedNames)]
    public QueryChangedNamesResponse QueryChangedNames(QueryChangedNamesRequest request)
    {
        logger.LogInformation("{Request}", request);
        var results = new Dictionary<Character, Character>();
        var characters = request.Characters.AsSpan();
        var charmap = new Dictionary<string, Dictionary<string, Character>>();
        foreach (var character in characters)
            if (nomenclatureService.Nomenclatures.TryGetValue(character, out var nomenclature))
            {
                Dictionary<string, Character>? worldmap = charmap.GetValueOrDefault(character.Name);
                worldmap ??= new Dictionary<string, Character>();
                worldmap[character.World] = nomenclature;
                charmap[character.Name] = worldmap;
            }
        return new QueryChangedNamesResponse() { Characters = charmap, Success = true };

    }
    
    private Character GetCharacterFromClaims()
    {
        var name = Context.User?.FindFirst(AuthClaimType.CharacterName)?.Value ?? throw new Exception("CharacterName is not present in claims");
        var world = Context.User?.FindFirst(AuthClaimType.WorldName)?.Value ?? throw new Exception("WorldName is not present in claims");
        return new Character(name, world);
    }
}