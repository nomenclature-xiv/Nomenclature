using System;
using System.Reflection;
using System.Threading.Tasks;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Nomenclature.Services;
using Nomenclature.UI;

namespace Nomenclature;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class Plugin : IDalamudPlugin
{
    // Constants
    private const string ChatCommandName = "/nom";
    
    // Dalamud Services
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] private static ICommandManager CommandManager { get; set; } = null!;
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static INamePlateGui NamePlateGui { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    
    // Configuration
    internal static Configuration Configuration { get; private set; } = null!;
    
    /// <summary>
    ///     Internal plugin version
    /// </summary>
    public static readonly Version Version =
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
    
    // Instantiated
    private readonly MainWindow _mainWindow = new();
    private readonly WindowSystem _windowSystem = new("Nomenclature");
    private readonly ScanningService _scanningService = new();
    private readonly IdentityService _identityService = new();

    /// <summary>
    ///     Entry point for the plugin
    /// </summary>
    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        
        _windowSystem.AddWindow(_mainWindow);
        
        PluginInterface.UiBuilder.Draw += Draw;
        PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;
        PluginInterface.UiBuilder.OpenConfigUi += OpenMainUi;

        CommandManager.AddHandler(ChatCommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "TODO"
        });
    }

    private void OnCommand(string command, string arguments) => _mainWindow.Toggle();

    private void Draw() => _windowSystem.Draw();

    private void OpenMainUi() => _mainWindow.IsOpen = true;
    
    /// <summary>
    ///     Runs provided function on the XIV Framework. Await should never be used inside the <see cref="Func{T}"/>
    ///     passed to this function.
    /// </summary>
    public static async Task<T> RunOnFramework<T>(Func<T> func)
    {
        if (Framework.IsInFrameworkUpdateThread)
            return func.Invoke();

        return await Framework.RunOnFrameworkThread(func).ConfigureAwait(false);
    }
    
    public void Dispose()
    {
        _scanningService.Dispose();
        _identityService.Dispose();
        
        PluginInterface.UiBuilder.Draw -= Draw;
        PluginInterface.UiBuilder.OpenMainUi -= OpenMainUi;
        PluginInterface.UiBuilder.OpenConfigUi -= OpenMainUi;
        
        CommandManager.RemoveHandler(ChatCommandName);
        
        _windowSystem.RemoveAllWindows();
    }
}