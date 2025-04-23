using System;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Nomenclature.Services;

/// <summary>
///     TODO
/// </summary>
public class NetworkService : IDisposable
{
    /// <summary>
    ///     TODO
    /// </summary>
    public readonly HubConnection Connection;

    /// <summary>
    ///     <inheritdoc cref="NetworkService"/>
    /// </summary>
    public NetworkService()
    {
        Connection = new HubConnectionBuilder()
            .WithUrl("", options =>
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

    public async Task<TU> InvokeAsync<T, TU>(string method, T request)
    {
        if (Connection.State is not HubConnectionState.Connected)
            return Activator.CreateInstance<TU>();

        try
        {
            Plugin.Log.Verbose($"[NetworkService] Request: {request}");
            var response = await Connection.InvokeAsync<TU>(method, request);
            Plugin.Log.Verbose($"[NetworkService] Response: {response}");
            return response;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[NetworkService] Unexpected error while invoking function on the server, {e}");
            return Activator.CreateInstance<TU>();
        }
    }

    public async Task Connect()
    {
        if (Connection.State is not HubConnectionState.Disconnected)
            return;

        try
        {
            Plugin.Log.Info("[NetworkService] Connecting...");
            await Connection.StartAsync().ConfigureAwait(false);
            Plugin.Log.Info($"[NetworkService] Connected: {Connection.State is HubConnectionState.Connected}");
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[NetworkService] Unexpected error while connecting, {e}");
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
            Plugin.Log.Warning($"[NetworkService] Unexpected error while disconnecting, {e}");
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
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}