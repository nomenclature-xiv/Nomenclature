using System;
using System.Net;
using System.Net.Http;
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
public class NetworkService : IHostedService, IDisposable
{
    /// <summary>
    ///     Signal R Hub Connection
    /// </summary>
    public readonly HubConnection Connection;

    /// <summary>
    ///     Event fired when the server successfully connects, either by reconnection or manual connection
    /// </summary>
    public event Func<Task>? Connected;

    /// <summary>
    ///     Event fired when the server connection is lost, either by disruption or manual intervention
    /// </summary>
    public event Func<Task>? Disconnected;

    public bool Connecting;

#if DEBUG
    private const string HubUrl = "https://localhost:5006/nomenclature";
    private const string AuthPostUrl = "https://localhost:5006/api/auth/login";
#else
    // Server URL 
    private const string HubUrl = "https://foxitsvc.com:5007/nomenclature";
     private const string AuthPostUrl = "https://foxitsvc.com:5007/api/auth/login";
#endif

    // Beta
    //private const string HubUrl = "https://foxitsvc.com:5017/nomenclature";
    //private const string AuthPostUrl = "https://foxitsvc.com:5017/api/auth/login";
    
    private readonly SessionService _sessionService;
    private readonly IPluginLog _pluginLog;

    private string? _token = string.Empty;

    /// <summary>
    ///     <inheritdoc cref="NetworkService"/>
    /// </summary>
    public NetworkService(SessionService sessionService, IPluginLog pluginLog)
    {
        _sessionService =  sessionService;
        _pluginLog = pluginLog;

        Connection = new HubConnectionBuilder()
            .WithUrl(HubUrl,
                options => { options.AccessTokenProvider = () => Task.FromResult<string?>(_token); })
            .WithAutomaticReconnect()
            .AddMessagePackProtocol(options =>
            {
                options.SerializerOptions =
                    MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);
            })
            .Build();

        Connection.Reconnected += OnReconnected;
        Connection.Reconnecting += OnReconnecting;
        Connection.Closed += OnClosed;
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
            if (await Token().ConfigureAwait(false) is { } token)
            {
                _token = token;
                await Connection.StartAsync().ConfigureAwait(false);
                if (Connection.State is HubConnectionState.Connected)
                {
                    Connected?.Invoke();
                    Connecting = false;
                }

            }
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
            Connecting = true;
        }
        catch (Exception e)
        {
            _pluginLog.Warning($"[NetworkService] Unexpected error while disconnecting, {e}");
        }
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Connecting = true;
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Disconnect();
        await Connection.DisposeAsync();
    }

    private Task OnReconnected(string? arg)
    {
        Connected?.Invoke();
        return Task.CompletedTask;
    }

    private Task OnClosed(Exception? arg)
    {
        Disconnected?.Invoke();
        return Task.CompletedTask;
    }

    private Task OnReconnecting(Exception? arg)
    {
        Disconnected?.Invoke();
        return Task.CompletedTask;
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

        return _sessionService.CurrentSession.CharacterConfiguration.Secret;
    }

    public void Dispose()
    {
        Connection.Reconnected -= OnReconnected;
        Connection.Reconnecting -= OnReconnecting;
        Connection.Closed -= OnClosed;

        Connection.StopAsync().ConfigureAwait(false);
        Connection.DisposeAsync().AsTask().GetAwaiter().GetResult();

        GC.SuppressFinalize(this);
    }
}