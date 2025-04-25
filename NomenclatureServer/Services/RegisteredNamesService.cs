using System.Security.Cryptography;

namespace NomenclatureServer.Services;

public class RegisteredNamesService
{
    /// <summary>
    ///     Dictionary mapping [CharacterName]@[HomeWorld] to [ModifiedCharacterName]
    /// </summary>
    public readonly Dictionary<string, string> ActiveNameChanges = new();
    
    /// <summary>
    ///     Dictionary containing any active registrations that have yet to be resolved
    ///     ValidationCode, LodestoneId
    /// </summary>
    public readonly Dictionary<string, string> PendingRegistrations = new();
    private readonly DatabaseService _databaseService;

    public RegisteredNamesService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<string?> RegisterName(string characterName)
    {
        string secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        bool res = await _databaseService.AddRegisteredCharacter(characterName, secret);

        if(res)
        {
            return secret;
        }
        return null;
    }
}