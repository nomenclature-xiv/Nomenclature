using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;
using Nomenclature.Types;

namespace Nomenclature;

[Serializable]
public class Configuration : IPluginConfiguration
{
    /// <summary>
    ///     Configuration version
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    ///     Name to replace real name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public List<BlocklistCharacter> BlocklistCharacters { get; set; } = new List<BlocklistCharacter>();

    [NonSerialized]
    private IDalamudPluginInterface? PluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
    }

    public void Save()
    {
        PluginInterface!.SavePluginConfig(this);
    }
}