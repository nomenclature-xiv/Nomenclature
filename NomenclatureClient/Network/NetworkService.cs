using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using MessagePack;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Services;
using NomenclatureClient.Types.Exceptions;
using NomenclatureCommon.Domain.Api.Controller;

namespace NomenclatureClient.Network;

/// <summary>
///     Provides access to the Signal R hub connection
/// </summary>
public class NetworkService : IHostedService
{
    // Server URL 
     private const string HubUrl = "https://foxitsvc.com:5007/nomenclature";
     private const string AuthPostUrl = "https://foxitsvc.com:5007/api/auth/login";
    
    // Beta
    //private const string HubUrl = "https://foxitsvc.com:5017/nomenclature";
    //private const string AuthPostUrl = "https://foxitsvc.com:5017/api/auth/login";
    
    public event Func<Task>? Connected;
    
    public int UserCount { get; set; }

    /// <summary>
    ///     Signal R Hub Connection
    /// </summary>
    public readonly HubConnection Connection;
    
    private readonly SessionService _sessionService;
    private readonly IPluginLog _pluginLog;

    /// <summary>
    ///     <inheritdoc cref="NetworkService"/>
    /// </summary>
    public NetworkService(SessionService sessionService, IPluginLog pluginLog)
    {
        _sessionService =  sessionService;
        _pluginLog = pluginLog;

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
    }

    /// <summary>
    ///     Invokes a method on the server and awaits a result
    /// </summary>
    /// <param name="method">The name of the method to call</param>
    /// <param name="request">The request object to send</param>
    /// <returns></returns>
    public async Task<T> InvokeAsync<T>(string method, object request)
    {
        if (Connection.State is not HubConnectionState.Connected)
        {
            _pluginLog.Warning("[NetworkService] No connection established");
            return Activator.CreateInstance<T>();
        }

        try
        {
            _pluginLog.Verbose($"[NetworkService] Request: {request}");
            var response = await Connection.InvokeAsync<T>(method, request);
            _pluginLog.Verbose($"[NetworkService] Response: {response}");
            return response;
        }
        catch (Exception e)
        {
            _pluginLog.Warning($"[NetworkService] [InvokeAsync] {e}");
            return Activator.CreateInstance<T>();
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
            if (Connection.State is HubConnectionState.Connected)
                Connected?.Invoke();
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
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Disconnect();
        await Connection.DisposeAsync();
    }

    /// <summary>
    ///     Queries the login server for a valid token
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidSecretException">Thrown when the client submits an invalid token</exception>
    /// <exception cref="UnknownTokenException">Thrown when the client gets an unexpected return code</exception>
    private async Task<string?> Token()
    {
        if (_sessionService.CurrentSession.CharacterConfiguration.Secret == string.Empty)
            throw new NoSecretForLocalCharacterException();

        var request = new GenerateTokenRequest(_sessionService.CurrentSession.CharacterConfiguration.Secret);
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
}