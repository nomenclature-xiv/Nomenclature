using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Microsoft.Extensions.Hosting;

namespace NomenclatureClient.UI;

public class WindowContainer : IHostedService
{
    // Injected
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly PrimaryWindow _primaryWindow;
    
    // Instantiated
    private readonly WindowSystem _windowSystem;

    public WindowContainer(IDalamudPluginInterface pluginInterface, PrimaryWindow primaryWindow)
    {
        _pluginInterface = pluginInterface;
        _primaryWindow = primaryWindow;
        _windowSystem = new WindowSystem("Nomenclature");
        _windowSystem.AddWindow(_primaryWindow);

#if DEBUG
        _primaryWindow.IsOpen = true;
#endif

        _pluginInterface.UiBuilder.Draw += OnDraw;
        _pluginInterface.UiBuilder.OpenMainUi += OnOpenMainUi;
        _pluginInterface.UiBuilder.OpenConfigUi += OnOpenMainUi;
    }
    
    private void OnDraw()
    {
        _windowSystem.Draw();
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