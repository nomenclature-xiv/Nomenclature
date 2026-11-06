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
using NomenclatureClient.Network;
using NomenclatureCommon.Domain.Api.Controller.Lodestone;
using NomenclatureCommon.Domain.Api.Controller.Oauth;

namespace NomenclatureClient.Network;

public class NetworkRegisterService(IPluginLog pluginLog, HttpClient client)
{
    public Func<Task>? Registered;

    public async Task<string?> RegisterCharacterXivAuth(Character character)
    {
        var resx = await RegisterCharacterInitiate<object, BeginCharacterRegistrationResponseOauth>("XIVAuth", null);
        if (resx == null)
            throw new Exception("Shouldn't happen!");
        Util.OpenLink(resx.Uri);
        //need to wait for polling to return secret
        return await RegisterCharacterPollXivAuth(resx.Ticket, character);
    }

    public async Task<string?> RegisterCharacterLodestone(Character character)
    {
        var request = new BeginCharacterRegistrationRequest()
        {
            Character = character
        };
        var resl = await RegisterCharacterInitiate<BeginCharacterRegistrationRequest, BeginCharacterRegistrationResponseLodestone>("Lodestone", request);
        if(resl == null)
            throw new Exception("Shouldn't happen!");
        //just return registration code, no polling because lodestone auth is expensive
        return resl.Code;
    }

    /// <summary>
    ///     Begins the registration process on the server
    /// </summary>
    /// 
    private async Task<TResult?> RegisterCharacterInitiate<T, TResult>(string method, T? request)
    {
        try
        {
            var response = await NetworkUtils.PostRequest(client, request is not null ? JsonSerializer.Serialize(request) : null, $"{NetworkConstants.BaseUrl}{NetworkConstants.RegisterPostUrlInit}/{method}");
            pluginLog.Verbose($"Registration request returned. Status: {response.StatusCode}");
            var text = await response.Content.ReadAsStringAsync();
            pluginLog.Verbose($"[RegisterCharacterInitiate] {text}");
            return JsonSerializer.Deserialize<TResult>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); 
        }
        catch (Exception e)
        {
            pluginLog.Warning($"[RegisterCharacterInitiate] {e}");
            return default;
        }
    }
    
    /// <summary>
    ///  Polls server for registration completion
    /// </summary>
    private async Task<string?> RegisterCharacterPollXivAuth(string ticket, Character character)
    {
        const int maxAttempts = 600 / 10; // Try once every 10 seconds for 10 minutes
        PollRegistrationResponse? lastPoll = null;
        ValidateCharacterRegistrationOauth data = new()
        {
            Ticket = ticket,
            Character = character
        };
        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                using var resp = await NetworkUtils.PostRequest(client, JsonSerializer.Serialize(data), $"{NetworkConstants.BaseUrl}{NetworkConstants.RegisterPostUrlValidate}/xivauth").ConfigureAwait(false);
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    var text = await resp.Content.ReadAsStringAsync();
                    var resmodel = JsonSerializer.Deserialize<ValidateCharacterRegistrationResponse>(text, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    if (resmodel?.Status is "bound")
                    {
                        return resmodel.Secret;
                    }
                    if(resmodel?.Status is not "unbound")
                    {
                        throw new Exception(resmodel?.Status);
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

    public async Task<string?> RegisterCharacterValidateLodestone(string validationCode)
    {
        try
        {
            var request = new ValidateCharacterRegistrationLodestone
            {
                Code = validationCode
            };
            var response = await NetworkUtils.PostRequest(client, JsonSerializer.Serialize(request), $"{NetworkConstants.BaseUrl}{NetworkConstants.RegisterPostUrlValidate}/lodestone");
            pluginLog.Verbose($"Validation request returned. Status: {response.StatusCode}");
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
