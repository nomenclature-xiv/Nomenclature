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
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Api.Controller;
using Nomenclature.Utils;
using System.Xml.Linq;
using System.Collections.Generic;
using Nomenclature.Services;

namespace Nomenclature.Network;

/// <summary>
///     TODO
/// </summary>
public class NetworkHubService : IHostedService
{
    // Server URL
    private const string HubUrl = "https://localhost:5006/nomenclature";

    // Post Url
    private const string AuthPostUrl = "https://localhost:5006/api/auth/login";

    /// <summary>
    ///     TODO
    /// </summary>
    public readonly HubConnection Connection;

    private readonly CharacterService _characterService;
    private readonly Configuration _configuration;
    private readonly IPluginLog PluginLog;

    /// <summary>
    ///     <inheritdoc cref="NetworkHubService"/>
    /// </summary>
    public NetworkHubService(IPluginLog pluginLog, Configuration configuration, CharacterService characterService)
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
        var name = _characterService.CurrentCharacter;
        if (name is null) return null;
        var secret = GetSecret(name);
        if (secret == null) return null;
        var request = new GenerateTokenRequest { Secret = secret };
        var response = await NetworkUtils.PostRequest(JsonSerializer.Serialize(request), AuthPostUrl);
        if (response.IsSuccessStatusCode is false)
            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new InvalidSecretException(),
                _ => new UnknownTokenException($"StatusCode was {response.StatusCode}")
            };
        
        PluginLog.Verbose("[NetworkHelper] Successfully authenticated");
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    private string? GetSecret(Character character)
    {
        _configuration.LocalCharacters.TryGetValue(character.Name, out Dictionary<string, string>? worldsecret);
        if(worldsecret is null)
        {
            return null;
        }
        worldsecret.TryGetValue(character.World, out string? secret);
        return secret;
    }
    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Disconnect().ConfigureAwait(false);
    }
}