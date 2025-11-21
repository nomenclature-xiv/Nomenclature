using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NomenclatureClient.UI;

public class WindowService(IDalamudPluginInterface pluginInterface, IEnumerable<Window> pluginWindows, WindowSystem windowSystem) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var pluginWindow in pluginWindows)
        {
            windowSystem.AddWindow(pluginWindow);

#if DEBUG
            if (pluginWindow.WindowName == "Debug") pluginWindow.IsOpen = true;
#endif
        }

        pluginInterface.UiBuilder.Draw += UiBuilderOnDraw;


        return Task.CompletedTask;
    }

    private void UiBuilderOnDraw()
    {
        windowSystem.Draw();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        pluginInterface.UiBuilder.Draw -= UiBuilderOnDraw;
        windowSystem.RemoveAllWindows();
        return Task.CompletedTask;
    }
}