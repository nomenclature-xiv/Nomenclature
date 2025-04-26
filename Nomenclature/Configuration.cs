using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Configuration;
using Dalamud.Plugin;
using Newtonsoft.Json;
using Nomenclature.Types;
using NomenclatureCommon.Domain;

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

    public List<Character> BlocklistCharacters { get; set; } = new List<Character>();

    public Dictionary<string, Dictionary<string, string>> LocalCharacters { get; set; } = new Dictionary<string, Dictionary<string, string>>();

    public bool AutoConnect = false;

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