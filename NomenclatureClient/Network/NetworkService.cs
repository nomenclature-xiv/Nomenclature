using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using MessagePack;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Services;

// ReSharper disable RedundantBoolCompare

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

    /// <summary>
    ///     If the plugin has begun the connection process
    /// </summary>
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
    
    // Injected
    private readonly IPluginLog _pluginLog;
    private readonly ConfigurationService _configuration;
    
    /// <summary>
    ///     Access token required to connect to the SignalR hub
    /// </summary>
    private string? _token = string.Empty;

    /// <summary>
    ///     <inheritdoc cref="NetworkService"/>
    /// </summary>
    public NetworkService(IPluginLog pluginLog, ConfigurationService configuration)
    {
        _pluginLog = pluginLog;
        _configuration = configuration;

        Connection = new HubConnectionBuilder().WithUrl(HubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_token);
            })
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
        
        Connecting = true;
        
        try
        {
            if (_configuration.CharacterConfiguration?.SecretId is not { } id)
                return;

            // TODO: Populate value once authentication secret code is fleshed out
            if (_configuration.Configuration.Secrets.TryGetValue(id, out _) is false)
                return;
            
            // TODO: Authenticate secret with server to get token
            _token = string.Empty;
            
            await Connection.StartAsync().ConfigureAwait(false);

            if (Connection.State is HubConnectionState.Connected)
            {
                Connected?.Invoke();
                // TODO: Connected Message
            }
            else
            {
                // TODO: Not Connected Message
            }
        }
        catch (Exception e)
        {
            _pluginLog.Warning($"[NetworkService.Connect] {e}");
        }
        
        Connecting = false;
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