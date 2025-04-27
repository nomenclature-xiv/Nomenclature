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
        var character = GetCharacterFromClaims();
        if (nomenclatureService.Nomenclatures.TryGetValue(character, out var existingNomenclature))
        {
            var name = request.Nomenclature.Name ?? existingNomenclature.Name;
            var world = request.Nomenclature.World ?? existingNomenclature.World;
            var nomenclature = new Nomenclature(name, world);
            nomenclatureService.Nomenclatures[character] = nomenclature;
        }
        else
        {
            nomenclatureService.Nomenclatures[character] = request.Nomenclature;
        }

        return new Response { Success = true };
    }

    [HubMethodName(ApiMethods.QueryChangedNames)]
    public QueryChangedNamesResponse QueryChangedNames(QueryChangedNamesRequest request)
    {
        logger.LogInformation("{Request}", request);

        var results = new List<CharacterIdentity>();
        var characters = request.Characters.AsSpan();
        foreach (var character in characters)
            if (nomenclatureService.Nomenclatures.TryGetValue(character, out var nomenclature))
                results.Add(new CharacterIdentity(character, nomenclature));

        return new QueryChangedNamesResponse { Success = true, Identities = results };
    }
    
    private Character GetCharacterFromClaims()
    {
        var name = Context.User?.FindFirst(AuthClaimType.CharacterName)?.Value ?? throw new Exception("CharacterName is not present in claims");
        var world = Context.User?.FindFirst(AuthClaimType.WorldName)?.Value ?? throw new Exception("WorldName is not present in claims");
        return new Character(name, world);
    }
}