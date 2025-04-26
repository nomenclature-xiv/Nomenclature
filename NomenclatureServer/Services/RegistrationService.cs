using System.Security.Cryptography;
using NomenclatureCommon.Domain;

namespace NomenclatureServer.Services;

public class RegistrationService(DatabaseService database, LodestoneService lodestoneService)
{
    /// <summary>
    ///     List of all current registrations
    /// </summary>
    private readonly Dictionary<string, string> _pendingRegistrations = new();

    /// <summary>
    ///     Begins the registration process by generating a validation code for a specific lodestone id
    /// </summary>
    /// <returns>The validation code</returns>
    public async Task<string?> BeginRegistrationAsync(Character character)
    {
        if (await lodestoneService.GetCharacterLodestoneIdAsync(character) is not { } lodestoneId)
            return null;
        
        var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        _pendingRegistrations[key] = lodestoneId;
        return key;
    }

    /// <summary>
    ///     Validates that the lodestone id has the provided validation code in the profile bio
    /// </summary>
    /// <returns>The secret for provided character</returns>
    public async Task<string?> ValidateRegistrationAsync(string validationCode)
    {
        if (_pendingRegistrations.TryGetValue(validationCode, out var lodestoneId) is false)
            return null;
        
        if (await lodestoneService.GetLodestoneCharacterByLodestoneId(lodestoneId) is not { } character)
            return null;

        if (character.Bio != validationCode)
            return null;
        
        _pendingRegistrations.Remove(validationCode);
        
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        if (await database.AddCharacter(secret, new Character(character.CharacterName, character.WorldName)))
            return secret;
        
        return null;
    }
}