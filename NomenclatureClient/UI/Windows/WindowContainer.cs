using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.UI.Views;

namespace NomenclatureClient.UI.Windows;

public class WindowContainer : IHostedService
{
    // Injected
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly PrimaryWindow _primaryWindow;
    private readonly PairsWindow _pairsWindow;
    private readonly SettingsWindow _settingsWindow;
    
    // Instantiated
    private readonly WindowSystem _windowSystem;

    public WindowContainer(IDalamudPluginInterface pluginInterface, PrimaryWindow primaryWindow, PairsWindow pairsWindow, SettingsWindow settingsWindow)
    {
        _pluginInterface = pluginInterface;
        _primaryWindow = primaryWindow;
        _pairsWindow = pairsWindow;
        _settingsWindow = settingsWindow;
        _windowSystem = new WindowSystem("Nomenclature");
        _windowSystem.AddWindow(_primaryWindow);
        _windowSystem.AddWindow(_pairsWindow);
        _windowSystem.AddWindow(_settingsWindow);

#if DEBUG
        _primaryWindow.IsOpen = true;
#endif

        _pluginInterface.UiBuilder.Draw += OnDraw;
        _pluginInterface.UiBuilder.OpenMainUi += OnOpenMainUi;
        _pluginInterface.UiBuilder.OpenConfigUi += OnOpenMainUi;
    }
    
    private void OnDraw()
    {
        BaseView.DrawTitle();
        _windowSystem.Draw();
        BaseView.Close();
    }
    
    private void OnOpenMainUi()
    {
        _primaryWindow.IsOpen = true;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _pluginInterface.UiBuilder.Draw -= OnDraw;
        _pluginInterface.UiBuilder.OpenMainUi -= OnOpenMainUi;
        _pluginInterface.UiBuilder.OpenConfigUi -= OnOpenMainUi;
        
        _windowSystem.RemoveAllWindows();
        return Task.CompletedTask;
    }
}