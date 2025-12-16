using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Network;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.UpdateSubscriptions;
using Timer = System.Timers.Timer;

namespace NomenclatureClient.Services;

// ReSharper disable ForCanBeConvertedToForeach

/// <summary>
///     Handles scanning nearby players to subscribe to on the server
/// </summary>
public class ScanningService(IFramework framework, IPluginLog logger, IObjectTable objectTable, NetworkService network) : IHostedService
{
    // Constants
    private const int ScanInternal = 5000;

    // Instantiated
    private readonly Timer _scanningTimer = new() { Interval = ScanInternal, Enabled = true };
    private ImmutableHashSet<string> _previousNearbyPlayers = [];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _scanningTimer.Elapsed += Scan;
        _scanningTimer.Start();

        network.Connection.Closed += OnServerConnectionClosed;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _scanningTimer.Elapsed -= Scan;
        _scanningTimer.Dispose();
        
        network.Connection.Closed -= OnServerConnectionClosed;
        return Task.CompletedTask;
    }
    
    /// <summary>
    ///     Scans the object table asynchronously every <see cref="ScanInternal"/> milliseconds
    /// </summary>
    private async void Scan(object? sender, ElapsedEventArgs _)
    {
        try
        {
            var nearby = await framework.RunOnFrameworkThread(ScanNearbyCharacters);
            var subscribeTo = nearby.Except(_previousNearbyPlayers).ToArray();
            var unsubscribeFrom = _previousNearbyPlayers.Except(nearby).ToArray();

            if (subscribeTo.Length is 0 && unsubscribeFrom.Length is 0)
                return;

            var request = new UpdateSubscriptionsRequest(subscribeTo, unsubscribeFrom);
            var response = await network.InvokeAsync<UpdateSubscriptionsResponse>(HubMethod.UpdateSubscriptions, request).ConfigureAwait(false);

            if (response.Success is false)
                return;

            // Remove those in the removed list
            foreach (var remove in unsubscribeFrom)
                IdentityService.Identities.TryRemove(remove, out var _);

            // Add those from the returned results
            foreach (var (name, nomenclature) in response.SubscribedNomenclatures)
                IdentityService.Identities[name] = nomenclature;

            // Assign a list
            _previousNearbyPlayers = nearby;
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
    private ImmutableHashSet<string> ScanNearbyCharacters()
    {
        var nearby = new HashSet<string>();
        for (var i = 0; i < objectTable.Length; i++)
        {
            if (objectTable[i] is not IPlayerCharacter player)
                continue;
                
            nearby.Add(string.Concat(player.Name.TextValue, "@", player.HomeWorld.Value.Name.ExtractText()));
        }
        
        return nearby.ToImmutableHashSet();
    }
    
    private Task OnServerConnectionClosed(Exception? exception)
    {
        _previousNearbyPlayers = [];
        return Task.CompletedTask;
    }
}