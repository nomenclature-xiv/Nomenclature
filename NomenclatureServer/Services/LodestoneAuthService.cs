using System.Collections.Concurrent;
using System.Security.Cryptography;
using NomenclatureCommon.Domain;
using NomenclatureServer.Domain;
using NomenclatureServer.Types;
using NomenclatureServer.Utilities;

namespace NomenclatureServer.Services;

public class LodestoneAuthService(LodestoneService lodestoneService, DatabaseService database)
{
    private static readonly ConcurrentDictionary<string, string> PendingRegistrations = new();

    public async Task<string?> InitLodestoneAuth(Character character)
    {
        if (await lodestoneService.GetCharacterLodestoneIdAsync(character) is not { } lodestoneId)
            return null;
        var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        PendingRegistrations[key] = lodestoneId;
        return key;
    }

    public async Task<LodestoneValidationResponse> ValidateLodestoneAuth(string code)
    {
        if (PendingRegistrations.TryGetValue(code, out var lodestoneId) is false)
            return new LodestoneValidationResponse()
            {
                Status = LodestoneValidationStatus.NoPendingRegistration,
                Secret = ""
            };

        if (await lodestoneService.GetLodestoneCharacterByLodestoneId(lodestoneId) is not { } character)
            return new LodestoneValidationResponse()
            {
                Status = LodestoneValidationStatus.CharacterNotFound,
                Secret = ""
            };

        if (character.Bio.Contains(code) is false)
            return new LodestoneValidationResponse()
            {
                Status = LodestoneValidationStatus.CodeNotFound,
                Secret = ""
            };

        PendingRegistrations.Remove(code, out _);

        var secret = AuthUtilities.GenerateSecret();
        var friendcode = AuthUtilities.GenerateFriendCode();
        var success = await database.CreateAccount(lodestoneId, friendcode, secret);
        if (success is not DatabaseResultEc.Success)
        {
            return new LodestoneValidationResponse()
            {
                Status = LodestoneValidationStatus.UnknownError,
                Secret = ""
            };
        }

        return new LodestoneValidationResponse()
        {
            Status = LodestoneValidationStatus.Success,
            Secret = secret
        };

    }
}