using System;
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
using NomenclatureClient.Services;
using NomenclatureCommon.Domain.Api.Login;

// ReSharper disable RedundantBoolCompare

namespace NomenclatureClient.Network;

/// <summary>
///     Provides access to the Signal R hub connection
/// </summary>
public class NetworkService : IHostedService
{
    // Const
    private static readonly JsonSerializerOptions DeserializationOptions = new() { PropertyNameCaseInsensitive = true };
    
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
                // ReSharper disable once RedundantTypeArgumentsOfMethod
                options.AccessTokenProvider = () => Task.FromResult<string?>(_token);
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
            // If we have a secret id set for this character
            if (_configuration.CharacterConfiguration?.SecretId is not { } id)
                return;

            // If the secret id corresponds to a valid secret
            if (_configuration.Configuration.Secrets.TryGetValue(id, out var secret) is false)
                return;

            // Try to authenticate it with the authentication server
            if (await TryAuthenticateSecret(secret).ConfigureAwait(false) is { } token)
            {
                _token = token;
                
                await Connection.StartAsync().ConfigureAwait(false);

                if (Connection.State is HubConnectionState.Connected)
                {
                    Connected?.Invoke();
                    _pluginLog.Info("[NetworkService.Connect] Successfully connected");
                }
                else
                {
                    _pluginLog.Info("[NetworkService.Connect] Unable to connect to the server");
                }
            }
        }
        catch (Exception e)
        {
            _pluginLog.Error($"[NetworkService.Connect] Unexpected error occurred, {e}");
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
        }
        catch (Exception e)
        {
            _pluginLog.Error($"[NetworkService.Disconnect] Unexpected error occurred, {e}");
        }
    }

    /// <summary>
    ///     <inheritdoc cref="IHostedService.StartAsync"/>
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Nothing needs to start immediately here. Auto connecting will happen in a different manager that calls the connect method
        return Task.CompletedTask;
    }

    /// <summary>
    ///     <inheritdoc cref="IHostedService.StopAsync"/>
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            Connection.Reconnected -= OnReconnected;
            Connection.Reconnecting -= OnReconnecting;
            Connection.Closed -= OnClosed;

            await Disconnect().ConfigureAwait(false);
            await Connection.DisposeAsync().ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Expected
        }
        catch (Exception e)
        {
            _pluginLog.Error($"[NetworkService.StopAsync] Unexpected error occurred, {e}");
        }
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
    
    private async Task<string?> TryAuthenticateSecret(string secret)
    {
        using var client = new HttpClient();
        var request = new LoginAuthenticationRequest(Plugin.Version, secret);
        var payload = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(AuthPostUrl, payload).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (JsonSerializer.Deserialize<LoginAuthenticationResponse>(content, DeserializationOptions) is not { } result)
            {
                _pluginLog.Warning("[NetworkService.TryAuthenticateSecret] A deserialization error occurred");
                return null;
            }
            
            switch (result.ErrorCode)
            {
                case LoginAuthenticationErrorCode.Success:
                    _pluginLog.Verbose("[NetworkService.TryAuthenticateSecret] Successfully authenticated");
                    return result.Secret;

                case LoginAuthenticationErrorCode.VersionMismatch:
                    _pluginLog.Warning("[NetworkService.TryAuthenticateSecret] Unsupported client version");
                    return null;

                case LoginAuthenticationErrorCode.UnknownSecret:
                    _pluginLog.Warning("[NetworkService.TryAuthenticateSecret] Invalid secret provided");
                    return null;

                case LoginAuthenticationErrorCode.Uninitialized:
                case LoginAuthenticationErrorCode.Unknown:
                default:
                    _pluginLog.Warning($"[NetworkService.TryAuthenticateSecret] Unknown error occurred on the server, {result.ErrorCode}");
                    return null;
            }
        }
        catch (HttpRequestException)
        {
            _pluginLog.Warning("[NetworkService.TryAuthenticateSecret] Authentication Server Down");
            return null;
        }
        catch (Exception e)
        {
            _pluginLog.Error($"[NetworkService.TryAuthenticateSecret] Unexpected error occurred, {e}");
            return null;
        }
    }
}