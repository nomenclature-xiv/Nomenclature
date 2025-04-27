using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;
using NomenclatureCommon.Domain;

namespace NomenclatureClient;

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
    public string World {  get; set; } = string.Empty;

    public bool SelfChangeName = true;

    /// <summary>
    ///     Should the server attempt to connect automatically?
    /// </summary>
    public bool AutoConnect = false;

    public List<Character> BlocklistCharacters { get; set; } = new List<Character>();

    public Dictionary<string, Dictionary<string, string>> LocalCharacters { get; set; } = new Dictionary<string, Dictionary<string, string>>();

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