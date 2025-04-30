using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NomenclatureClient.Network;
using NomenclatureClient.UI;
using NomenclatureClient.UI.New;
using MainWindowController = NomenclatureClient.UI.New.MainWindowController;

namespace NomenclatureClient.Services
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
            IDataManager dataManager,
            IChatGui chatGui,
            ITextureProvider textureProvider
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
                    collection.AddSingleton(chatGui);
                    collection.AddSingleton(textureProvider);
                    collection.AddSingleton<CharacterService>();
                    collection.AddSingleton<IdentityService>();
                    collection.AddSingleton<ScanningService>();
                    collection.AddSingleton<FrameworkService>();
                    collection.AddSingleton<CommandService>();
                    collection.AddSingleton<WorldService>();
                    collection.AddSingleton<NetworkHubService>();
                    collection.AddSingleton<NetworkRegisterService>();
                    collection.AddSingleton<NetworkNameService>();
                    collection.AddSingleton<FontService>();
                    collection = AddUiServices(collection);

                    //Services to automatically start when the plugin does
                    collection.AddHostedService(p => p.GetRequiredService<IdentityService>());
                    collection.AddHostedService(p => p.GetRequiredService<ScanningService>());
                    collection.AddHostedService(p => p.GetRequiredService<WindowService>());
                    collection.AddHostedService(p => p.GetRequiredService<InstallerWindowService>());
                    collection.AddHostedService(p => p.GetRequiredService<CommandService>());
                    collection.AddHostedService(p => p.GetRequiredService<NetworkHubService>());
                    collection.AddHostedService(p => p.GetRequiredService<FontService>());
                }).Build();

        }
        private static IServiceCollection AddUiServices(IServiceCollection collection)
        {
            collection.AddSingleton<WindowService>();
            collection.AddSingleton<InstallerWindowService>();
            collection.AddSingleton<RegistrationWindowController>();
            collection.AddSingleton<RegistrationWindow>();
            collection.AddSingleton<MainWindowController>();
            collection.AddSingleton<MainWindow>();

            //Easier to do using autofac
            collection.AddSingleton<Window>(provider => provider.GetRequiredService<RegistrationWindow>());
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
