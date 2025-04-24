using System;
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Nomenclature;

namespace Nomenclature.Services;

/// <summary>
///     Provides a list of all in game world names accessible to the player
/// </summary>
public class WorldService
{
    private readonly ExcelSheet<World> _worldSheet;
    private readonly IPluginLog PluginLog;

    /// <summary>
    ///     List of all in game world names accessible to the player
    /// </summary>
    public readonly List<string> WorldNames;

    public WorldService(IDataManager dataManager, IPluginLog pluginLog)
    {
        _worldSheet = dataManager.Excel.GetSheet<World>();

        var worldList = new List<string>();
        for (uint i = 0; i < _worldSheet.Count; i++)
        {
            var world = _worldSheet.GetRowOrDefault(i);
            if (world is null) continue;

            var name = world.Value.InternalName.ToString();
            if (ShouldIncludeWorld(name))
                worldList.Add(name);
        }

        worldList.Sort();
        WorldNames = [.. worldList];
        PluginLog = pluginLog;
    }

    /// <summary>
    ///     Attempts to get a world name by world id
    /// </summary>
    /// <param name="worldId">
    ///     The world id provided by
    ///     <see cref="FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara.HomeWorld" />
    /// </param>
    /// <returns>World name, or null if not found</returns>
    public string? TryGetWorldById(ushort worldId)
    {
        try
        {
            var world = _worldSheet.GetRowOrDefault(worldId);
            if (world is not null)
                return world.Value.InternalName.ToString();

            PluginLog.Warning($"[WorldProvider] World {worldId} not found.");
            return null;
        }
        catch (Exception ex)
        {
            PluginLog.Warning($"[WorldProvider] Error during world lookup: {ex}");
            return null;
        }
    }

    /// <summary>
    ///     Various worlds are developer, promotional, or incomplete and must be filtered out
    /// </summary>
    /// <param name="world"></param>
    /// <returns></returns>
    private static bool ShouldIncludeWorld(string world)
    {
        if (world == string.Empty) return false;
        if (char.IsUpper(world[0]) is false) return false;
        if (world == "Dev") return false;
        for (var i = world.Length - 1; i >= 0; i--)
            if (char.IsDigit(world[i]))
                return false;

        return true;
    }
}