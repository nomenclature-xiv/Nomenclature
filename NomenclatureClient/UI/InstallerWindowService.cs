using Dalamud.Plugin;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using NomenclatureClient.UI.New;

namespace NomenclatureClient.UI;

public class InstallerWindowService(IDalamudPluginInterface pluginInterface, MainWindow mainWindow) : IHostedService {
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        pluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        pluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;
        return Task.CompletedTask;
    }

    private void ToggleMainUi()
    {
        mainWindow.Toggle();
    }

    private void ToggleConfigUi()
    {
        mainWindow.Toggle();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        pluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        pluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        return Task.CompletedTask;
    }
}
