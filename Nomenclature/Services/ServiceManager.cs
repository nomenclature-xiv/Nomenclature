using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nomenclature.UI;

namespace Nomenclature.Services
{
    internal class ServiceManager
    {
        public static IHost RegisterServices(IDalamudPluginInterface pluginInterface, 
            ICommandManager commandManager,
            IClientState clientState,
            IFramework framework,
            INamePlateGui namePlateGui,
            IObjectTable objectTable,
            IPluginLog pluginLog,
            IDataManager dataManager
            )
        {
            return new HostBuilder()
                .UseContentRoot(pluginInterface.ConfigDirectory.FullName)
                .ConfigureLogging(lb =>
                {
                    lb.ClearProviders();
                    lb.SetMinimumLevel(LogLevel.Trace);
                })
                .ConfigureServices(collection =>
                {
                    //Add dalamud services
                    collection.AddSingleton(pluginInterface);
                    collection.AddSingleton(commandManager);
                    collection.AddSingleton(clientState);
                    collection.AddSingleton(framework);
                    collection.AddSingleton(namePlateGui);
                    collection.AddSingleton(objectTable);
                    collection.AddSingleton(pluginLog);
                    collection.AddSingleton(dataManager);
                    collection.AddSingleton<IdentityService>();
                    collection.AddSingleton<ScanningService>();
                    collection.AddSingleton<FrameworkService>();
                    collection.AddSingleton<CommandService>();
                    collection.AddSingleton<WorldService>();
                    collection = AddUiServices(collection);

                    //Services to automatically start when the plugin does
                    collection.AddHostedService(p => p.GetRequiredService<IdentityService>());
                    collection.AddHostedService(p => p.GetRequiredService<ScanningService>());
                    collection.AddHostedService(p => p.GetRequiredService<WindowService>());
                    collection.AddHostedService(p => p.GetRequiredService<InstallerWindowService>());
                    collection.AddHostedService(p => p.GetRequiredService<CommandService>());
                }).Build();

        }
        private static IServiceCollection AddUiServices(IServiceCollection collection)
        {
            collection.AddSingleton<WindowService>();
            collection.AddSingleton<InstallerWindowService>();
            collection.AddSingleton<MainWindowController>();
            collection.AddSingleton<MainWindow>();

            //Easier to do using autofac
            collection.AddSingleton<Window>(provider => provider.GetRequiredService<MainWindow>());

            //Add configuration
            collection.AddSingleton((s) =>
            {
                var dalamudPluginInterface = s.GetRequiredService<IDalamudPluginInterface>();
                var configuration = dalamudPluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
                configuration.Initialize(dalamudPluginInterface);
                return configuration;
            });

            //Add window system
            collection.AddSingleton(new WindowSystem("Nomenclature"));
            return collection;
        }
    }
}
