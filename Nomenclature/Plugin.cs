using System;
using System.Reflection;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using Nomenclature.Services;

namespace Nomenclature;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class Plugin : IDalamudPlugin
{
    // Constants
    private readonly IHost _host;
    
    /// <summary>
    ///     Internal plugin version
    /// </summary>
    public static readonly Version Version =
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);

    /// <summary>
    ///     Entry point for the plugin
    /// </summary>
    public Plugin(IDalamudPluginInterface pluginInterface,
            ICommandManager commandManager,
            IClientState clientState,
            IFramework framework,
            INamePlateGui namePlateGui,
            IObjectTable objectTable,
            IPluginLog pluginLog,
            IDataManager dataManager,
            IChatGui chatGui)
    {
        _host = ServiceManager.RegisterServices(pluginInterface, commandManager, clientState, framework, namePlateGui, objectTable, pluginLog, dataManager, chatGui);

        _ = _host.StartAsync();
    }
    
    public void Dispose()
    {
        _host.StopAsync().GetAwaiter().GetResult();
        _host.Dispose();
    }
}