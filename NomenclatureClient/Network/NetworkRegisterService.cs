using Dalamud.Plugin.Services;
using System.Text.Json;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Api.Controller;
using System;
using System.Threading.Tasks;
using Dalamud.Utility;
using Microsoft.Win32;
using System.Net.Http;
using System.Net;
using NomenclatureCommon.Domain.Exceptions;

namespace NomenclatureClient.Network;

public class NetworkRegisterService(IPluginLog pluginLog, HttpClient client)
{
    private const string RegisterPostUrlInit = "https://localhost:5006/registration/initiate";
    private const string RegisterPostUrlPoll = "https://localhost:5006/registration/validate";

    public Func<Task>? Registered;

    public async Task<string?> RegisterCharacter(Character character)
    {
        var res = await RegisterCharacterInitiate(character);
        if (res == null)
            throw new Exception("Shouldn't happen!");
        Util.OpenLink(res.Uri);
        return await RegisterCharacterPoll(res.Ticket, character);
    }

    /// <summary>
    ///     Begins the registration process on the server
    /// </summary>
    /// 
    private async Task<BeginCharacterRegistrationResponse?> RegisterCharacterInitiate(Character characterName)
    {
        try
        {
            var request = new BeginCharacterRegistrationRequest
            {
                Character = characterName
            };
            var response = await NetworkUtils.PostRequest(client, JsonSerializer.Serialize(request), RegisterPostUrlInit);
            pluginLog.Verbose($"Registration request returned. Status: {response.StatusCode}");
            var text = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<BeginCharacterRegistrationResponse>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); 
        }
        catch (Exception e)
        {
            pluginLog.Warning($"[RegisterCharacterInitiate] {e}");
            return null;
        }
    }
    
    /// <summary>
    ///  Polls server for registration completion
    /// </summary>
    private async Task<string?> RegisterCharacterPoll(string ticket, Character character)
    {
        const int maxAttempts = 600 / 10; // Try once every 10 seconds for 10 minutes
        PollRegistrationResponse? lastPoll = null;
        ValidateCharacterRegistration data = new()
        {
            Ticket = ticket,
            Character = character
        };
        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                using var resp = await NetworkUtils.PostRequest(client, JsonSerializer.Serialize(data), RegisterPostUrlPoll).ConfigureAwait(false);
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    var text = await resp.Content.ReadAsStringAsync();
                    var resmodel = JsonSerializer.Deserialize<ValidateCharacterRegistrationResponse>(text, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    if (resmodel?.Status is "bound")
                    {
                        return resmodel.Token;
                    }
                }
                if (resp.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new CharacterNotMatchingException(await resp.Content.ReadAsStringAsync());
                }
            }
            catch(HttpRequestException ex)
            {
                pluginLog.Warning(ex.Message);
            }
            
            await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        }
        return null;
    }
}
