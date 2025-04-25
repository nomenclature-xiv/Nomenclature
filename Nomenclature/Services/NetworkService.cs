using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using MessagePack;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nomenclature.Types.Exceptions;
using NomenclatureCommon.Api;

namespace Nomenclature.Services;

/// <summary>
///     TODO
/// </summary>
public class NetworkService : IHostedService
{
    // Server Url
    private const string HubUrl = "https://localhost:5006/nomenclature";
    
    // Post Url
    private const string AuthPostUrl = "https://localhost:5006/api/auth/login";
    private const string RegisterPostUrlInit = "https://localhost:5006/registration/initiate";
    private const string RegisterPostUrlValidate = "https://localhost:5006/registration/validate";
    
    /// <summary>
    ///     TODO
    /// </summary>
    public readonly HubConnection Connection;

    private readonly CharacterService _characterService;
    private readonly Configuration _configuration;
    private readonly IPluginLog PluginLog;

    /// <summary>
    ///     <inheritdoc cref="NetworkService"/>
    /// </summary>
    public NetworkService(IPluginLog pluginLog, Configuration configuration, CharacterService characterService)
    {
        PluginLog = pluginLog;
        _configuration = configuration;
        _characterService = characterService;

        Connection = new HubConnectionBuilder()
            .WithUrl(HubUrl, options =>
            {
                options.AccessTokenProvider = async () => await Token().ConfigureAwait(false);
            })
            .WithAutomaticReconnect()
            .AddMessagePackProtocol(options =>
            {
                options.SerializerOptions = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);
            })
            .Build();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        PluginLog.Verbose("Starting server...");
        await Connect().ConfigureAwait(false);
    }

    public async Task<TU> InvokeAsync<T, TU>(string method, T request)
    {
        if (Connection.State is not HubConnectionState.Connected)
        {
            PluginLog.Verbose($"[NetworkService] Cannot invoke method {method} because connection is not connected]");
            return Activator.CreateInstance<TU>();
        }

        try
        {
            PluginLog.Verbose($"[NetworkService] Request: {request}");
            var response = await Connection.InvokeAsync<TU>(method, request);
            PluginLog.Verbose($"[NetworkService] Response: {response}");
            return response;
        }
        catch (Exception e)
        {
            PluginLog.Warning($"[NetworkService] Unexpected error while invoking function on the server, {e}");
            return Activator.CreateInstance<TU>();
        }
    }

    public async Task Connect()
    {
        if (Connection.State is not HubConnectionState.Disconnected)
            return;

        try
        {
            await Connection.StartAsync().ConfigureAwait(false);
        }
        catch (InvalidSecretException)
        {
            PluginLog.Warning("[NetworkService] Your secret is invalid. Make sure your current character is registered");
        }
        catch (Exception e)
        {
            PluginLog.Warning($"[NetworkService] Unexpected error while connecting, {e}");
        }
    }

    public async Task Disconnect()
    {
        if (Connection.State is HubConnectionState.Disconnected)
            return;

        try
        {
            await Connection.StopAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            PluginLog.Warning($"[NetworkService] Unexpected error while disconnecting, {e}");
        }
    }

    /// <summary>
    ///     Queries the login server for a valid token
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidSecretException">Thrown when the client submits an invalid token</exception>
    /// <exception cref="UnknownTokenException">Thrown when the client gets an unexpected return code</exception>
    private async Task<string?> Token()
    {
        var name = await _characterService.GetCurrentCharacter()!;
        if (name.CharacterName is null || name.WorldName is null) return null;
        string charworld = name.CharacterName + "@" + name.WorldName;
        _configuration.LocalCharacters.TryGetValue(charworld, out string? secret);
        if (secret == null) return null;
        var request = new TokenRequest { Secret = secret };
        var response = await PostRequest(JsonSerializer.Serialize(request), AuthPostUrl);
        if (response.IsSuccessStatusCode is false)
            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new InvalidSecretException(),
                _ => new UnknownTokenException($"StatusCode was {response.StatusCode}")
            };
        
        PluginLog.Verbose("[NetworkHelper] Successfully authenticated");
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     Begins the registration process on the server
    /// </summary>
    public async Task<string?> RegisterCharacterInitiate(string characterName, string worldName)
    {
        try
        {
            var request = new RegisterCharacterInitiateRequest
            {
                CharacterName = characterName,
                WorldName = worldName
            };
            var response = await PostRequest(JsonSerializer.Serialize(request), RegisterPostUrlInit);
            return response.IsSuccessStatusCode 
                ? await response.Content.ReadAsStringAsync().ConfigureAwait(false) 
                : null;
        }
        catch (Exception e)
        {
            PluginLog.Warning($"[RegisterCharacterInitiate] {e}");
            return null;
        }
    }

    public async Task<string?> RegisterCharacterValidate(string characterName, string validationCode)
    {
        try
        {
            var request = new RegisterCharacterValidateRequest
            {
                CharacterName = characterName,
                ValidationCode = validationCode
            };
            var response = await PostRequest(JsonSerializer.Serialize(request), RegisterPostUrlValidate);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                : null;
        }
        catch (Exception e)
        {
            PluginLog.Warning($"[RegisterCharacterInitiate] {e}");
            return null;
        }
    }

    /// <summary>
    ///     Posts a request to the server
    /// </summary>
    private static async Task<HttpResponseMessage> PostRequest(string content, string url)
    {
        using var client = new HttpClient();
        var payload = new StringContent(content, Encoding.UTF8, "application/json");
        return await client.PostAsync(url, payload).ConfigureAwait(false);
    }
    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Disconnect().ConfigureAwait(false);
    }
}