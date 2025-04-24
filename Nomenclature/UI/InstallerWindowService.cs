using Dalamud.Plugin;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nomenclature.UI
{
    public class InstallerWindowService : IHostedService
    {
        public IDalamudPluginInterface PluginInterface { get; }
        public MainWindow MainWindow { get; }

        public InstallerWindowService(IDalamudPluginInterface pluginInterface, MainWindow mainWindow)
        {
            PluginInterface = pluginInterface;
            MainWindow = mainWindow;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
            PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

            return Task.CompletedTask;
        }

        private void ToggleMainUi()
        {
            MainWindow.Toggle();
        }

        private void ToggleConfigUi()
        {
            MainWindow.Toggle();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
            PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
            return Task.CompletedTask;
        }
    }
}
