using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;
using NomenclatureClient.Types;

namespace NomenclatureClient;

[Serializable]
public class Configuration : IPluginConfiguration
{
    [NonSerialized]
    private IDalamudPluginInterface? _pluginInterface;
    
    /// <summary>
    ///     Configuration version
    /// </summary>
    public int Version { get; set; } = 3;
    
    /// <summary>
    ///     Map of local characters to their configurations. Local characters are in the format of Name@World
    /// </summary>
    public Dictionary<string, CharacterConfiguration> LocalConfigurations = [];
    
    /// <summary>
    ///     Save the configuration
    /// </summary>
    public void Save()
    {
        _pluginInterface!.SavePluginConfig(this);
    }
    
    /// <summary>
    ///     Initialize the configuration. This must be called before the config can be successfully used
    /// </summary>
    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
    }
}