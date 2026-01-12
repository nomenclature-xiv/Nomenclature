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
using System.Net.Http;

namespace NomenclatureClient.Managers;

public static class ServiceManager
{
    public static IHost RegisterServices(
        IChatGui chatGui,
        IClientState clientState,
        ICommandManager commandManager,
        IDalamudPluginInterface pluginInterface, 
        IDataManager dataManager,
        IFramework framework,
        INamePlateGui namePlateGui,
        IObjectTable objectTable,
        IPluginLog pluginLog)
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
                collection.AddSingleton<HttpClient>();

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
                collection.AddSingleton<WorldService>();
                collection.AddSingleton<NetworkService>();
                collection.AddSingleton<NetworkRegisterService>();
                collection.AddSingleton<FontService>();
                collection.AddSingleton<IpcTester>();
                collection.AddSingleton<ConfigurationService>();
                
                // Managers
                collection.AddSingleton<IpcManager>();
                collection.AddSingleton<LoginManager>();
                collection.AddSingleton<NomenclatureManager>();
                
                // Handlers
                collection.AddSingleton<ChatBoxHandler>();
                collection.AddSingleton<NamePlateHandler>();
                collection.AddSingleton<CommandHandler>();
                
                // Network Handlers
                collection.AddSingleton<RemoveNomenclatureHandler>();
                collection.AddSingleton<UpdateNomenclatureHandler>();
                collection.AddSingleton<UpdateOnlineStatusHandler>();
                collection.AddSingleton<NetworkHandler>();
                
                // UI
                collection = AddUiServices(collection);

                //Services to automatically start when the plugin does
                collection.AddHostedService(p => p.GetRequiredService<ConfigurationService>());
                collection.AddHostedService(p => p.GetRequiredService<WindowService>());
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
        collection.AddSingleton<IpcWindow>();
        collection.AddSingleton<PrimaryWindow>();
        collection.AddSingleton<Window>(provider => provider.GetRequiredService<IpcWindow>());

        //Easier to do using autofac
        collection.AddSingleton<Window>(provider => provider.GetRequiredService<PrimaryWindow>());

        collection.AddSingleton(async provider =>
        {
            var dalamudPluginInterface = provider.GetRequiredService<IDalamudPluginInterface>();
            var logging = provider.GetRequiredService<IPluginLog>();
            var configuration = new ConfigurationService(dalamudPluginInterface, logging);
            await configuration.LoadConfigurationAsync();
            return configuration;
        });

        //Add a window system
        collection.AddSingleton(new WindowSystem("Nomenclature"));
        return collection;
    }
}

