﻿using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NomenclatureClient.Debug;
using NomenclatureClient.Ipc;
using NomenclatureClient.Network;
using NomenclatureClient.Services.New;
using NomenclatureClient.UI;
using NomenclatureClient.UI.New;

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
            IChatGui chatGui
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
                    collection.AddSingleton<CharacterService>();
                    collection.AddSingleton<LoginService>();
                    collection.AddSingleton<IdentityService>();
                    collection.AddSingleton<NameService>();
                    collection.AddSingleton<ScanningService>();
                    collection.AddSingleton<FrameworkService>();
                    collection.AddSingleton<CommandService>();
                    collection.AddSingleton<WorldService>();
                    collection.AddSingleton<NetworkHubService>();
                    collection.AddSingleton<NetworkRegisterService>();
                    collection.AddSingleton<NetworkNameService>();
                    collection.AddSingleton<FontService>();
                    collection.AddSingleton<NameplateHandlerService>();
                    collection.AddSingleton<ChatBoxHandlerService>();
                    collection.AddSingleton<IpcManager>();
                    collection.AddSingleton<IpcTester>();
                    collection = AddUiServices(collection);

                    //Services to automatically start when the plugin does
                    collection.AddHostedService(p => p.GetRequiredService<LoginService>());
                    collection.AddHostedService(p => p.GetRequiredService<IdentityService>());
                    collection.AddHostedService(p => p.GetRequiredService<ScanningService>());
                    collection.AddHostedService(p => p.GetRequiredService<WindowService>());
                    collection.AddHostedService(p => p.GetRequiredService<InstallerWindowService>());
                    collection.AddHostedService(p => p.GetRequiredService<CommandService>());
                    collection.AddHostedService(p => p.GetRequiredService<NetworkHubService>());
                    collection.AddHostedService(p => p.GetRequiredService<FontService>());
                    collection.AddHostedService(p => p.GetRequiredService<NameplateHandlerService>());
                    collection.AddHostedService(p => p.GetRequiredService<ChatBoxHandlerService>());
                    collection.AddHostedService(p => p.GetRequiredService<IpcManager>());
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
            collection.AddSingleton<BlocklistWindowController>();
            collection.AddSingleton<BlocklistWindow>();
            collection.AddSingleton<IpcWindow>();
            collection.AddSingleton<Window>(provider => provider.GetRequiredService<IpcWindow>());

            //Easier to do using autofac
            collection.AddSingleton<Window>(provider => provider.GetRequiredService<RegistrationWindow>());
            collection.AddSingleton<Window>(provider => provider.GetRequiredService<BlocklistWindow>());
            collection.AddSingleton<Window>(provider => provider.GetRequiredService<MainWindow>());

            //Add configuration
            collection.AddSingleton((s) =>
            {
                var dalamudPluginInterface = s.GetRequiredService<IDalamudPluginInterface>();
                var configuration = dalamudPluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
                configuration.Initialize(dalamudPluginInterface);
                configuration.Migrate();
                return configuration;
            });

            //Add window system
            collection.AddSingleton(new WindowSystem("Nomenclature"));
            return collection;
        }
    }
}
