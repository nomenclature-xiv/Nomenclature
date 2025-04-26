using Microsoft.Extensions.Logging;
using NetStone;
using NetStone.Search.Character;
using System.Security.Cryptography;
using Microsoft.Extensions.Hosting;
using NomenclatureCommon.Domain;
using NomenclatureServer.Domain;

namespace NomenclatureServer.Services;

public class LodestoneService(ILogger<LodestoneService> logger) : IHostedService
{
    // The lodestone client will only be created from inside the hosted service, so we can assign this null for now
    private LodestoneClient _client = null!;

    /// <summary>
    ///     Initialize the <see cref="LodestoneClient"/>
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client = await LodestoneClient.GetClientAsync();
    }
    
    /// <summary>
    ///     Dispose the <see cref="LodestoneClient"/>
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Searches for a character on the lodestone and returns the corresponding lodestone id if found
    /// </summary>
    public async Task<string?> GetCharacterLodestoneIdAsync(Character character)
    {
        var query = new CharacterSearchQuery
        {
            CharacterName = character.Name,
            World = character.World
        };

        try
        {
            // If the results null or doesn't have any results
            if ((await _client.SearchCharacter(query)) is not { } results || results.HasResults is false)
                return null;
            
            // Iterate over the results and look for the correct character name
            foreach (var lodestoneCharacter in results.Results)
                if (lodestoneCharacter.Name == character.Name && lodestoneCharacter.Id is { } lodestoneId)
                    return lodestoneId;
        }
        catch (Exception e)
        {
            logger.LogError("{Exception}", e);
        }
        
        return null;
    }

    /// <summary>
    ///     Searches for a character by lodestone id and returns their bio if found
    /// </summary>
    public async Task<LodestoneCharacterData?> GetLodestoneCharacterByLodestoneId(string lodestoneId)
    {
        try
        {
            if (await _client.GetCharacter(lodestoneId) is not { } character)
                return null;

            return new LodestoneCharacterData(character.Name, character.Server, character.Bio);
        }
        catch (Exception e)
        {
            logger.LogError("{Exception}", e);
            return null;
        }
    }
}