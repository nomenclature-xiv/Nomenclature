using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;
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
    public int Version { get; set; } = 1;

    /// <summary>
    ///     Should the server attempt to connect automatically?
    /// </summary>
    public bool AutoConnect = false;

    /// <summary>
    ///     Map of [Character]@[World] to Secret
    /// </summary>
    public readonly Dictionary<string, string> LocalCharacterSecrets = new();
    
    /// <summary>
    ///     List of [Character]@[World] the local client has blocked
    /// </summary>
    public readonly List<Character> BlocklistCharacters = [];
    
    public void Save()
    {
        _pluginInterface!.SavePluginConfig(this);
    }
}