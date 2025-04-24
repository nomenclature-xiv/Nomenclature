using Dalamud.Interface.Windowing;
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
    public class WindowService : IHostedService
    {
        public IDalamudPluginInterface PluginInterface { get; }
        public IEnumerable<Window> PluginWindows { get; }
        public WindowSystem WindowSystem { get; }

        public WindowService(IDalamudPluginInterface pluginInterface, IEnumerable<Window> pluginWindows, WindowSystem windowSystem)
        {
            PluginInterface = pluginInterface;
            PluginWindows = pluginWindows;
            WindowSystem = windowSystem;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var pluginWindow in PluginWindows)
            {
                WindowSystem.AddWindow(pluginWindow);
                
                #if DEBUG
                pluginWindow.IsOpen = true;
                #endif

            }

            PluginInterface.UiBuilder.Draw += UiBuilderOnDraw;


            return Task.CompletedTask;
        }

        private void UiBuilderOnDraw()
        {
            WindowSystem.Draw();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            PluginInterface.UiBuilder.Draw -= UiBuilderOnDraw;
            WindowSystem.RemoveAllWindows();
            return Task.CompletedTask;
        }
    }
}
