using System;
using Dalamud.Configuration;

namespace Nomenclature;

[Serializable]
public class Configuration : IPluginConfiguration
{
    /// <summary>
    ///     Configuration version
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    ///     Save configuration
    /// </summary>
    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);
}