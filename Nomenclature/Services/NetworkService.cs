using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using MessagePack;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Nomenclature.Services;

/// <summary>
///     TODO
/// </summary>
public class NetworkService : IHostedService
{
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
            .WithUrl("https://localhost:5006", options =>
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

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task<TU> InvokeAsync<T, TU>(string method, T request)
    {
        if (Connection.State is not HubConnectionState.Connected)
            return Activator.CreateInstance<TU>();

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

    private static async Task<string?> Token()
    {
        try
        {
            return await Task.FromResult("TODO").ConfigureAwait(false);
        }
        catch (Exception)
        {
            return await Task.FromResult("TODO, but EVIL").ConfigureAwait(false);
        }
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}