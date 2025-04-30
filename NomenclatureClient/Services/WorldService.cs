using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace NomenclatureClient.Services;

/// <summary>
///     Provides a list of all in game world names accessible to the player
/// </summary>
public class WorldService
{
    /// <summary>
    ///     List of all in game world names accessible to the player
    /// </summary>
    public readonly List<string> WorldNames;

    /// <summary>
    ///     <inheritdoc cref="WorldService"/>
    /// </summary>
    public WorldService(IDataManager dataManager)
    {
        var worldSheet = dataManager.Excel.GetSheet<World>();

        var worldList = new List<string>();
        for (uint i = 0; i < worldSheet.Count; i++)
        {
            var world = worldSheet.GetRowOrDefault(i);
            if (world is null) continue;

            var name = world.Value.InternalName.ToString();
            if (ShouldIncludeWorld(name))
                worldList.Add(name);
        }

        worldList.Sort();
        WorldNames = [.. worldList];
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