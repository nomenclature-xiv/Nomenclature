using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Timers;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace Nomenclature.Services;

/// <summary>
///     TODO
/// </summary>
public class ScanningService : IDisposable
{
    // Constants
    private const int ScanInternal = 1000; //15000;
    
    // Instantiated
    private readonly Timer _scanningTimer;

    /// <summary>
    ///     <inheritdoc cref="ScanningService"/>
    /// </summary>
    public ScanningService()
    {
        _scanningTimer = new Timer { Interval = ScanInternal, Enabled = true };
        _scanningTimer.Elapsed += Scan;
        _scanningTimer.Start();
    }

    /// <summary>
    ///     Scans the object table asynchronously every <see cref="ScanInternal"/> milliseconds
    /// </summary>
    private async void Scan(object? sender, ElapsedEventArgs _)
    {
        try
        {
            var stop = Stopwatch.StartNew();
            Plugin.Log.Verbose("Beginning Scan...");
            
            await Plugin.RunOnFramework(Scan).ConfigureAwait(false);
            
            Plugin.Log.Verbose("Finished Scan...");
            stop.Stop();
            Plugin.Log.Verbose($"Scan took { stop.ElapsedTicks * 1000000 / Stopwatch.Frequency } microseconds ({stop.ElapsedMilliseconds} ms)");
        }
        catch (Exception e)
        {
            Plugin.Log.Fatal($"Unexpected issue occurred while scanning, {e}");
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
        for (var i = 0; i < Plugin.ObjectTable.Length; i++)
        {
            if (Plugin.ObjectTable[i] is not IPlayerCharacter player)
                continue;
            
            _identityNameBuilder.Clear();
            _identityNameBuilder.Append(player.Name);
            _identityNameBuilder.Append('@');
            _identityNameBuilder.Append(player.HomeWorld.Value.Name);
            players.Add(_identityNameBuilder.ToString());
        }

        return players;
    }

    public void Dispose()
    {
        _scanningTimer.Dispose();
        GC.SuppressFinalize(this);
    }
}