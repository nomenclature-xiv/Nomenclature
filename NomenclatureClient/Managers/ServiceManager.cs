using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NomenclatureClient.Debug;
using NomenclatureClient.Handlers;
using NomenclatureClient.Handlers.Network;
using NomenclatureClient.Ipc;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureClient.UI;

namespace NomenclatureClient.Managers;

public static class ServiceManager
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
                // Dalamud Services 
                collection.AddSingleton(pluginInterface);
                collection.AddSingleton(commandManager);
                collection.AddSingleton(clientState);
                collection.AddSingleton(framework);
                collection.AddSingleton(namePlateGui);
                collection.AddSingleton(objectTable);
                collection.AddSingleton(pluginLog);
                collection.AddSingleton(dataManager);
                collection.AddSingleton(chatGui);
                
                // Internal Services
                collection.AddSingleton<SessionService>();
                collection.AddSingleton<IdentityService>();
                collection.AddSingleton<ScanningService>();
                
                collection.AddSingleton<WorldService>();
                collection.AddSingleton<NetworkService>();
                collection.AddSingleton<NetworkRegisterService>();
                collection.AddSingleton<FontService>();
                
                
                
                
                
                collection.AddSingleton<IpcTester>();
                
                // Managers
                collection.AddSingleton<IpcManager>();
                collection.AddSingleton<IdentityManager>();
                collection.AddSingleton<LoginManager>();
                
                // Handlers
                collection.AddSingleton<ChatBoxHandler>();
                collection.AddSingleton<NamePlateHandler>();
                collection.AddSingleton<CommandHandler>();
                
                // Network Handlers
                collection.AddSingleton<DeleteNomenclatureHandler>();
                collection.AddSingleton<UpdateNomenclatureHandler>();
                collection.AddSingleton<UpdateUserCountHandler>();
                collection.AddSingleton<NetworkHandler>();
                
                // UI
                collection = AddUiServices(collection);

                //Services to automatically start when the plugin does
                collection.AddHostedService(p => p.GetRequiredService<IdentityService>());
                collection.AddHostedService(p => p.GetRequiredService<ScanningService>());
                collection.AddHostedService(p => p.GetRequiredService<WindowService>());
                collection.AddHostedService(p => p.GetRequiredService<InstallerWindowService>());
                collection.AddHostedService(p => p.GetRequiredService<CommandHandler>());
                collection.AddHostedService(p => p.GetRequiredService<NetworkService>());
                collection.AddHostedService(p => p.GetRequiredService<FontService>());
                collection.AddHostedService(p => p.GetRequiredService<NamePlateHandler>());
                collection.AddHostedService(p => p.GetRequiredService<ChatBoxHandler>());
                collection.AddHostedService(p => p.GetRequiredService<IpcManager>());
                collection.AddHostedService(p => p.GetRequiredService<LoginManager>());
                collection.AddHostedService(p => p.GetRequiredService<NetworkHandler>());
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
            return configuration;
        });

        //Add a window system
        collection.AddSingleton(new WindowSystem("Nomenclature"));
        return collection;
    }
}

