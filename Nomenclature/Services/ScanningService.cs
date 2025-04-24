using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;

namespace Nomenclature.Services;

/// <summary>
///     TODO
/// </summary>
public class ScanningService : IHostedService
{
    private readonly IPluginLog PluginLog;
    private readonly IObjectTable ObjectTable;
    private readonly FrameworkService FrameworkService;
    // Constants
    private const int ScanInternal = 1000; //15000;
    
    // Instantiated
    private readonly System.Timers.Timer _scanningTimer;

    /// <summary>
    ///     <inheritdoc cref="ScanningService"/>
    /// </summary>
    public ScanningService(IPluginLog pluginLog, IObjectTable objectTable, FrameworkService frameworkService)
    {
        PluginLog = pluginLog;
        FrameworkService = frameworkService;
        ObjectTable = objectTable;

        _scanningTimer = new System.Timers.Timer { Interval = ScanInternal, Enabled = true };
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
            
            await FrameworkService.RunOnFramework(Scan).ConfigureAwait(false);
            
            //PluginLog.Verbose("Finished Scan...");
            stop.Stop();
            //PluginLog.Verbose($"Scan took { stop.ElapsedTicks * 1000000 / Stopwatch.Frequency } microseconds ({stop.ElapsedMilliseconds} ms)");
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
    private List<string> Scan()
    {
        var players = new List<string>();
        
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < ObjectTable.Length; i++)
        {
            if (ObjectTable[i] is not IPlayerCharacter player)
                continue;
            
            _identityNameBuilder.Clear();
            _identityNameBuilder.Append(player.Name);
            _identityNameBuilder.Append('@');
            _identityNameBuilder.Append(player.HomeWorld.Value.Name);
            players.Add(_identityNameBuilder.ToString());
        }

        return players;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _scanningTimer.Dispose();
        return Task.CompletedTask;
    }
}