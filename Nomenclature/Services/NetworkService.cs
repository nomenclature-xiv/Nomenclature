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
using NomenclatureCommon.Domain.Api;

namespace Nomenclature.Services;

/// <summary>
///     TODO
/// </summary>
public class NetworkService : IHostedService
{
    private const string HubUrl = "https://localhost:5006/nomenclature";
    private const string PostUrl = "https://localhost:5006/api/auth/login";
    
    /// <summary>
    ///     TODO
    /// </summary>
    public readonly HubConnection Connection;

    private readonly IPluginLog PluginLog;

    /// <summary>
    ///     <inheritdoc cref="NetworkService"/>
    /// </summary>
    public NetworkService(IPluginLog pluginLog)
    {
        PluginLog = pluginLog;

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
            PluginLog.Info("[NetworkService] Connecting...");
            await Connection.StartAsync().ConfigureAwait(false);
            PluginLog.Info($"[NetworkService] Connected: {Connection.State is HubConnectionState.Connected}");
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

    private async Task<string?> Token()
    {
        try
        {
            using var client = new HttpClient();
            var request = new TokenRequest
            {
                Secret = "Misty"
            };

            var payload = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(PostUrl, payload).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                PluginLog.Verbose("[NetworkHelper] Successfully authenticated");
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            var error = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "[NetworkHelper] Unable to authenticate, invalid secret",
                HttpStatusCode.BadRequest => "[NetworkHelper] Unable to authenticate, outdated client",
                _ => $"[NetworkHelper] Unable to authenticate, {response.StatusCode}"
            };

            PluginLog.Warning(error);
            return null;
        }
        catch (Exception e)
        {
            PluginLog.Warning($"[NetworkHelper] Unable to send POST to server, {e.Message}");
            return null;
        }
    }
    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Disconnect().ConfigureAwait(false);
    }
}