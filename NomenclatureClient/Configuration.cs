using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;
using NomenclatureClient.Types;
using NomenclatureCommon.Domain;

namespace NomenclatureClient;

[Serializable]
public class Configuration : IPluginConfiguration
{
    [NonSerialized] private IDalamudPluginInterface? _pluginInterface;
    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
    }
    
    /// <summary>
    ///     Configuration version
    /// </summary>
    public int Version { get; set; } = 2;

    /// <summary>
    ///     Should the server attempt to connect automatically?
    /// </summary>
    public bool AutoConnect = false;

    /// <summary>
    ///     [deprecated] Map of [Character]@[World] to Secret
    /// </summary>
    public Dictionary<string, string> LocalCharacterSecrets = new();

    /// <summary>
    ///     Map of [Character]@[World] to all per-character config values, including secrets.
    /// </summary>
    public readonly Dictionary<string, CharConfig> LocalCharacters = new();
    
    /// <summary>
    ///     List of [Character]@[World] the local client has blocked
    /// </summary>
    public readonly List<Character> BlocklistCharacters = [];
    
    public void Save()
    {
        _pluginInterface!.SavePluginConfig(this);
    }

    public void Migrate()
    {
        if(Version == 1)
        {
            foreach(string key in LocalCharacterSecrets.Keys)
            {
                LocalCharacters.Add(key, new CharConfig() { Secret = LocalCharacterSecrets[key] });
            }
            Version = 2;
        }
        Save();
    }
}