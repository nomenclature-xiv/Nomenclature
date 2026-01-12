using System;
using System.Reflection;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Managers;

namespace NomenclatureClient;

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
    public Plugin(
        IChatGui chatGui,
        IClientState clientState,
        ICommandManager commandManager,
        IDalamudPluginInterface pluginInterface, 
        IDataManager dataManager,
        IFramework framework,
        INamePlateGui namePlateGui,
        IObjectTable objectTable,
        IPluginLog pluginLog)
    {
        _host = ServiceManager.RegisterServices(chatGui, clientState, commandManager, pluginInterface, dataManager, framework, namePlateGui, objectTable, pluginLog);
        _ = _host.StartAsync();
    }
    
    public void Dispose()
    {
        _host.StopAsync().GetAwaiter().GetResult();
        _host.Dispose();
    }
}