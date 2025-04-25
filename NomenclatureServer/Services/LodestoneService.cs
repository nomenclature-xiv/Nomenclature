using Microsoft.Extensions.Logging;
using NetStone;
using NetStone.Search.Character;

namespace NomenclatureServer.Services;

public class LodestoneService(RegisteredNamesService registeredNamesService, ILogger<LodestoneService> logger)
{
    private LodestoneClient? _client = null;

    public async Task<LodestoneErrorCode> Initiate(string characterName, string worldName)
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
                return LodestoneErrorCode.CharacterNotFound;

            if (results.HasResults is false)
                return LodestoneErrorCode.CharacterNotFound;

            foreach (var character in results.Results)
            {
                if (character.Name == characterName)
                {
                    logger.LogInformation("Got a hit!");
                    if (await character.GetCharacter() is { } dl)
                        logger.LogInformation($"{dl.Name} - {dl.Bio}");
                }
                else
                {
                    logger.LogInformation("Got a miss!");
                }
            }
            
            return LodestoneErrorCode.Success;

        }
        catch (Exception e)
        {
            logger.LogError("{Exception}", e);
            return LodestoneErrorCode.UnknownException;
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