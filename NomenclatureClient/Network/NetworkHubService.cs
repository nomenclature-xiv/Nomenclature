using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using MessagePack;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NomenclatureClient.Services;
using NomenclatureClient.Services.New;
using NomenclatureClient.Types.Exceptions;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Api;
using NomenclatureCommon.Domain.Api.Controller;

namespace NomenclatureClient.Network;

/// <summary>
///     Provides access to the Signal R hub connection
/// </summary>
public class NetworkHubService : IHostedService
{
    // Server URL
    private const string HubUrl = "https://foxitsvc.com:5017/nomenclature";

    // Post Url
    private const string AuthPostUrl = "https://foxitsvc.com:5017/api/auth/login";

    public int UserCount;

    /// <summary>
    ///     Signal R Hub Connection
    /// </summary>
    public readonly HubConnection Connection;

    private readonly CharacterService _characterService;
    private readonly IPluginLog _pluginLog;
    private readonly INamePlateGui _namePlateGui;

    /// <summary>
    ///     <inheritdoc cref="NetworkHubService"/>
    /// </summary>
    public NetworkHubService(IPluginLog pluginLog, INamePlateGui namePlateGui, CharacterService characterService)
    {
        _pluginLog = pluginLog;
        _namePlateGui = namePlateGui;
        _characterService = characterService;

        Connection = new HubConnectionBuilder()
            .WithUrl(HubUrl,
                options => { options.AccessTokenProvider = async () => await Token().ConfigureAwait(false); })
            .WithAutomaticReconnect()
            .AddMessagePackProtocol(options =>
            {
                options.SerializerOptions =
                    MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);
            })
            .Build();
        AddClientMethods();
        
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _pluginLog.Verbose("Starting server...");
        await Connect().ConfigureAwait(false);
    }

    /// <summary>
    ///     Invokes a generic on the SignalR hub
    /// </summary>
    /// <param name="method">See <see cref="ApiMethods"/> for available methods</param>
    /// <param name="request">The request object matching type <see cref="T"/></param>
    /// <typeparam name="T">The request object type</typeparam>
    /// <typeparam name="TU">The response object type</typeparam>
    /// <returns>The response <see cref="TU"/> or <see cref="Nullable"/></returns>
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

    /// <summary>
    ///     Starts a connection to the Signal R server
    /// </summary>
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
            _pluginLog.Warning(
                "[NetworkService] You do not have a secret assigned to your local character, please register");
        }
        catch (InvalidSecretException)
        {
            _pluginLog.Warning(
                "[NetworkService] Your secret is invalid. Make sure your current character is registered");
        }
        catch (Exception e)
        {
            _pluginLog.Warning($"[NetworkService] Unexpected error while connecting, {e}");
        }
    }

    /// <summary>
    ///     Stops a connection to the Signal R server
    /// </summary>
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
        if(_characterService.CurrentConfig is not { } config)
            throw new NoSecretForLocalCharacterException();

        var request = new GenerateTokenRequest { Secret = config.Secret };
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
        Connection.On<string, Nomenclature>(ApiMethods.UpdateNomenclatureEvent, (charactername, nomenclature) =>
        {
            _pluginLog.Debug($"Updated Nomenclature for {charactername} to {nomenclature}");
            IdentityService.Identities[charactername] = nomenclature;
            _namePlateGui.RequestRedraw();
        });

        Connection.On<string>(ApiMethods.RemoveNomenclatureEvent, (charactername) =>
        {
            _pluginLog.Debug($"Clearing Nomenclature for {charactername}");
            IdentityService.Identities.Remove(charactername);
            _namePlateGui.RequestRedraw();
        });
        Connection.On<int>(ApiMethods.UpdateUserCountEvent, (usercount) =>
        {
            _pluginLog.Verbose($"Current user count: {usercount}");
            this.UserCount = usercount;
        });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Disconnect().ConfigureAwait(false);
    }
}