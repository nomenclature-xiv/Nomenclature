using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Configuration;
using Dalamud.Plugin;
using NomenclatureClient.Types;
using NomenclatureCommon.Domain;

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
    /// [deprecated] Should the client attempt to automatically connect to the server?
    /// </summary>
    public bool AutoConnect = false;

    /// <summary>
    /// [deprecated] Map of [Character]@[World] to all per-character config values, including secrets
    /// </summary>
    public Dictionary<string, CharConfig> LocalCharacters = new();

    /// <summary>
    ///     Map of local characters to their configurations. Local characters are in the format of Name@World
    /// </summary>
    public Dictionary<string, CharacterConfiguration> LocalConfigurations = [];

    /// <summary>
    ///     [deprecated] List of [Character]@[World] the local client has blocked
    /// </summary>
    public readonly List<Character> BlocklistCharacters = [];

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
        if (Version == 2)
        {
            foreach(string key in LocalCharacters.Keys)
            {
                var value = LocalCharacters[key];
                LocalConfigurations.Add(key, new CharacterConfiguration()
                {
                    AutoConnect = AutoConnect,
                    OverrideName = value.UseName,
                    OverrideWorld = value.UseWorld,
                    Name = value.Name,
                    World = value.World
                });
            }
            Version = 3;
            Save();
        }
    }
}