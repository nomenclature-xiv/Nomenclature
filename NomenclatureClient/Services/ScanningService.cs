using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Network;
using NomenclatureClient.Services.New;
using NomenclatureCommon.Domain.Api;
using NomenclatureCommon.Domain.Api.Server;
using Microsoft.AspNetCore.SignalR.Client;
using Timer = System.Timers.Timer;

namespace NomenclatureClient.Services;

// ReSharper disable once ForCanBeConvertedToForeach

/// <summary>
///     Handles scanning nearby players to subscribe to on the server
/// </summary>
public class ScanningService(
    IPluginLog logger,
    IObjectTable objectTable,
    FrameworkService framework,
    NetworkHubService network) : IHostedService
{
    // Constants
    private const int ScanInternal = 5000;

    // Instantiated
    private readonly Timer _scanningTimer = new() { Interval = ScanInternal, Enabled = true };
    private HashSet<string> _previousNearbyPlayers = [];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _scanningTimer.Elapsed += Scan;
        _scanningTimer.Start();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Scans the object table asynchronously every <see cref="ScanInternal"/> milliseconds
    /// </summary>
    private async void Scan(object? sender, ElapsedEventArgs _)
    {
        try
        {
            if(network.Connection.State == HubConnectionState.Disconnected)
            {
                IdentityService.Identities.Clear();
                _previousNearbyPlayers.Clear();
                return;
            }
            var nearbyPlayers = await framework.RunOnFramework(ScanNearbyCharacters).ConfigureAwait(false);
            var added = nearbyPlayers.Except(_previousNearbyPlayers).ToArray();
            var removed = _previousNearbyPlayers.Except(nearbyPlayers).ToArray();

            if (added.Length is 0 && removed.Length is 0)
                return;

            var req = new SyncNomenclatureUpdateSubscriptionsRequest
            {
                CharacterIdentitiesToSubscribeTo = added,
                CharacterIdentitiesToUnsubscribeFrom = removed
            };

            var response =
                await network
                    .InvokeAsync<SyncNomenclatureUpdateSubscriptionsRequest,
                        SyncNomenclatureUpdateSubscriptionsResponse>(ApiMethods.SyncNomenclatureUpdateSubscriptions,
                        req);

            if (response.Success is false)
                return;

            // Remove those in the removed list
            foreach (var remove in removed)
                IdentityService.Identities.Remove(remove);

            // Add those from the returned results
            foreach (var (name, nomenclature) in response.NewlySubscribedNomenclatures)
                IdentityService.Identities[name] = nomenclature;

            // Assign a list
            _previousNearbyPlayers = nearbyPlayers;
        }
        catch (Exception e)
        {
            logger.Fatal($"Unexpected issue occurred while scanning, {e}");
        }
    }

    /// <summary>
    ///     Puts all the player characters nearby the local player into a list with format [CharacterName]@[HomeWorld]
    /// </summary>
    /// <returns></returns>
    private HashSet<string> ScanNearbyCharacters()
    {
        var nearby = new HashSet<string>();
        for (var i = 0; i < objectTable.Length; i++)
        {
            if (objectTable[i] is not IPlayerCharacter player)
                continue;

            nearby.Add(string.Concat(player.Name.TextValue, "@", player.HomeWorld.Value.Name.ExtractText()));
        }

        return nearby;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _scanningTimer.Dispose();
        return Task.CompletedTask;
    }
}