using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Api;
using NomenclatureCommon.Domain.Api.Base;
using NomenclatureCommon.Domain.Api.Server;

namespace NomenclatureClient.Services;

/// <summary>
///     TODO
/// </summary>
public class ScanningService : IHostedService
{
    private readonly IPluginLog PluginLog;
    private readonly IObjectTable ObjectTable;
    private readonly IClientState _clientState;
    private readonly FrameworkService FrameworkService;
    private readonly NetworkHubService NetworkService;
    private readonly IdentityService IdentityService;
    private readonly Configuration Configuration;
    // Constants
    private const int ScanInternal = 5000; //15000;
    
    // Instantiated
    private readonly System.Timers.Timer _scanningTimer;
    private List<Character> _characterList;

    /// <summary>
    ///     <inheritdoc cref="ScanningService"/>
    /// </summary>
    public ScanningService(IPluginLog pluginLog, IObjectTable objectTable, IClientState clientState, FrameworkService frameworkService, NetworkHubService networkService, IdentityService identityService, Configuration configuration)
    {
        PluginLog = pluginLog;
        FrameworkService = frameworkService;
        ObjectTable = objectTable;
        NetworkService = networkService;
        IdentityService = identityService;
        _clientState = clientState;
        Configuration = configuration;

        _scanningTimer = new System.Timers.Timer { Interval = ScanInternal, Enabled = true };
        _characterList = new List<Character>();
    }

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
            var stop = Stopwatch.StartNew();
            //PluginLog.Verbose("Beginning Scan...");
            
            List<Character> localNames = await FrameworkService.RunOnFramework(Scan).ConfigureAwait(false);
            
            //PluginLog.Verbose("Finished Scan...");
            stop.Stop();
            //PluginLog.Verbose($"Scan took { stop.ElapsedTicks * 1000000 / Stopwatch.Frequency } microseconds ({stop.ElapsedMilliseconds} ms)");

            List<string> posdiff = (from character in localNames.Except(_characterList) select character.ToString()).ToList();
            List<Character> negchars = _characterList.Except(localNames).ToList();
            List<string> negdiff = (from character in negchars select character.ToString()).ToList();
            if (posdiff.Count == 0 && negdiff.Count == 0)
            {
                return;
            }
            SyncNomenclatureUpdateSubscriptionsRequest request = new()
            {
                namesToSubscribeTo = posdiff,
                namesToUnsubscribeTo = negdiff
            };
            foreach(Character character in negchars)
            {
                if(IdentityService.Identities.TryGetValue(character, out var nomenclature))
                {
                    IdentityService.Identities.Remove(character);
                }
            }
            
            var res = await NetworkService.InvokeAsync<SyncNomenclatureUpdateSubscriptionsRequest, SyncNomenclatureUpdateSubscriptionsResponse>(ApiMethods.SyncNomenclatureUpdateSubscriptions, request); //response.ModifiedNames;
            if(res.Success)
            {
                _characterList = localNames;
                foreach(var identity in res.newlySubscribedNomenclatures)
                {
                    IdentityService.Identities[Character.FromString(identity.Key)] = identity.Value;
                }
            }
           
        }
        catch (Exception e)
        {
            PluginLog.Fatal($"Unexpected issue occurred while scanning, {e}");
        }
    }

    private readonly StringBuilder _identityNameBuilder = new();
    
    /// <summary>
    ///     Puts all the player characters nearby the local player into a list with format [CharacterName]@[HomeWorld]
    /// </summary>
    /// <returns></returns>
    private List<Character> Scan()
    {
        var players = new List<Character>();
        
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < ObjectTable.Length; i++)
        {
            if (ObjectTable[i] is not IPlayerCharacter player)
                continue;
            
            players.Add(new Character(player.Name.ToString(), player.HomeWorld.Value.Name.ToString()));
        }

        return players;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _scanningTimer.Dispose();
        return Task.CompletedTask;
    }
}