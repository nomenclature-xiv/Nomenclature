using Dalamud.Plugin.Services;
using System.Text.Json;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Api.Controller;
using System;
using System.Threading.Tasks;

namespace NomenclatureClient.Network;

public class NetworkRegisterService(IPluginLog pluginLog)
{
    private const string RegisterPostUrlInit = "https://localhost:5006/registration/initiate";
    private const string RegisterPostUrlValidate = "https://localhost:5006/registration/validate";

    /// <summary>
    ///     Begins the registration process on the server
    /// </summary>
    public async Task<string?> RegisterCharacterInitiate(Character characterName)
    {
        try
        {
            var request = new BeginCharacterRegistrationRequest
            {
                Character = characterName
            };
            var response = await NetworkUtils.PostRequest(JsonSerializer.Serialize(request), RegisterPostUrlInit);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                : null;
        }
        catch (Exception e)
        {
            pluginLog.Warning($"[RegisterCharacterInitiate] {e}");
            return null;
        }
    }

    public async Task<string?> RegisterCharacterValidate(string validationCode)
    {
        try
        {
            var request = new ValidateCharacterRegistration
            {
                ValidationCode = validationCode
            };
            var response = await NetworkUtils.PostRequest(JsonSerializer.Serialize(request), RegisterPostUrlValidate);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                : null;
        }
        catch (Exception e)
        {
            pluginLog.Warning($"[RegisterCharacterInitiate] {e}");
            return null;
        }
    }
}
