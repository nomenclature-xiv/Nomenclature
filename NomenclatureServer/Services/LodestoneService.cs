using Microsoft.Extensions.Logging;
using NetStone;
using NetStone.Search.Character;
using System.Security.Cryptography;

namespace NomenclatureServer.Services;

public class LodestoneService(RegisteredNamesService registeredNamesService, ILogger<LodestoneService> logger)
{
    private LodestoneClient? _client = null;

    public async Task<string?> Initiate(string characterName, string worldName)
    {
        try
        {
            _client ??= await LodestoneClient.GetClientAsync();

            var query = new CharacterSearchQuery
            {
                CharacterName = characterName,
                World = worldName
            };

            if (await _client.SearchCharacter(query) is not { } results)
                return null;

            if (results.HasResults is false)
                return null;

            foreach (var character in results.Results)
            {
                if (character.Name == characterName)
                {
                    logger.LogInformation("Got a hit!");
                    if (await character.GetCharacter() is { } dl)
                    {
                        string lodestoneId = character.Id!;
                        string key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                        registeredNamesService.PendingRegistrations[lodestoneId] = key;
                        return key;
                    }
                }
                else
                {
                    logger.LogInformation("Got a miss!");
                }
            }

            return null;

        }
        catch (Exception e)
        {
            logger.LogError("{Exception}", e);
            return null;
        }
    }
    
    public async Task<LodestoneErrorCode> Validate(string lodestoneId)
    {
        try
        {
            _client ??= await LodestoneClient.GetClientAsync();

            if (registeredNamesService.PendingRegistrations.TryGetValue(lodestoneId, out var validationCode) is false)
                return LodestoneErrorCode.NoActiveValidation;

            if (await _client.GetCharacter(lodestoneId) is not { } character)
                return LodestoneErrorCode.CharacterNotFound;

            return character.Bio == validationCode
                ? LodestoneErrorCode.Success
                : LodestoneErrorCode.IncorrectValidationCode;
        }
        catch (Exception e)
        {
            logger.LogError("{Exception}", e);
            return LodestoneErrorCode.UnknownException;
        }
    }
}

public enum LodestoneErrorCode
{
    Success,
    NoActiveValidation,
    CharacterNotFound,
    IncorrectValidationCode,
    UnknownException
}