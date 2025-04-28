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
using NomenclatureClient.Types.Exceptions;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Api.Controller;
using System.Collections.Generic;
using NomenclatureClient.Services;
using ImGuiScene;
using NomenclatureCommon.Domain.Api;

namespace NomenclatureClient.Network;

/// <summary>
///     Provides access to the Signal R hub connection
/// </summary>
public class NetworkHubService : IHostedService
{
    // Server URL
    private const string HubUrl = "https://localhost:5006/nomenclature";

    // Post Url
    private const string AuthPostUrl = "https://localhost:5006/api/auth/login";

    /// <summary>
    ///     Signal R Hub Connection
    /// </summary>
    public readonly HubConnection Connection;

    private readonly CharacterService _characterService;
    private readonly Configuration _configuration;
    private readonly IdentityService _identityService;
    private readonly IPluginLog _pluginLog;

    /// <summary>
    ///     <inheritdoc cref="NetworkHubService"/>
    /// </summary>
    public NetworkHubService(IPluginLog pluginLog, Configuration configuration, CharacterService characterService, IdentityService identityService)
    {
        _pluginLog = pluginLog;
        _configuration = configuration;
        _characterService = characterService;
        _identityService = identityService;

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
        AddClientMethods();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _pluginLog.Verbose("Starting server...");
        await Connect().ConfigureAwait(false);
    }

    public async Task<TU> InvokeAsync<T, TU>(string method, T request)
    {
        if (Connection.State is not HubConnectionState.Connected)
        {
            _pluginLog.Verbose($"[NetworkService] Cannot invoke method {method} because connection is not connected]");
            return Activator.CreateInstance<TU>();
        }

        try
        {
            _pluginLog.Verbose($"[NetworkService] Request: {request}");
            var response = await Connection.InvokeAsync<TU>(method, request);
            _pluginLog.Verbose($"[NetworkService] Response: {response}");
            return response;
        }
        catch (Exception e)
        {
            _pluginLog.Warning($"[NetworkService] Unexpected error while invoking function on the server, {e}");
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
        catch (NoSecretForLocalCharacterException)
        {
            _pluginLog.Warning("[NetworkService] You do not have a secret assigned to your local character, please register");
        }
        catch (InvalidSecretException)
        {
            _pluginLog.Warning("[NetworkService] Your secret is invalid. Make sure your current character is registered");
        }
        catch (Exception e)
        {
            _pluginLog.Warning($"[NetworkService] Unexpected error while connecting, {e}");
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
            _pluginLog.Warning($"[NetworkService] Unexpected error while disconnecting, {e}");
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
        if (_characterService.CurrentCharacter is not { } currentCharacter)
            return null;
        
        if (GetCharacterSecret(currentCharacter) is not { } secret)
            throw new NoSecretForLocalCharacterException();
        
        var request = new GenerateTokenRequest { Secret = secret };
        var response = await NetworkUtils.PostRequest(JsonSerializer.Serialize(request), AuthPostUrl);
        if (response.IsSuccessStatusCode is false)
            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new InvalidSecretException(),
                _ => new UnknownTokenException($"StatusCode was {response.StatusCode}")
            };
        
        _pluginLog.Verbose("[NetworkHelper] Successfully authenticated");
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    private void AddClientMethods()
    {
        Connection.On<Character, Nomenclature>(ApiMethods.UpdateNomenclature, (character, nomenclature) =>
        {
            _identityService.Identities[character] = nomenclature;
        });
        Connection.On<Character>(ApiMethods.RemoveNomenclature, (character) =>
        {
            if(_identityService.Identities.ContainsKey(character))
                _identityService.Identities.Remove(character);
        });
    }

    private string? GetCharacterSecret(Character character)
    {
        return _configuration.LocalCharacters.TryGetValue(character.Name, out var worlds)
            ? worlds.GetValueOrDefault(character.World)
            : null;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Disconnect().ConfigureAwait(false);
    }
}