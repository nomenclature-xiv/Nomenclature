using System;
using System.Net.Http;
using System.Reflection;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NomenclatureClient.Handlers;
using NomenclatureClient.Handlers.Network;
using NomenclatureClient.Managers;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureClient.UI;
using NomenclatureClient.UI.Views.Login;
using NomenclatureClient.UI.Views.Manage;
using NomenclatureClient.UI.Views.Nomenclature;
using NomenclatureClient.UI.Views.Pairs;
using NomenclatureClient.UI.Views.Register;
using NomenclatureClient.UI.Views.Settings;

namespace NomenclatureClient;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class Plugin : IDalamudPlugin
{
    // Constants
    private readonly IHost _host;
    
    /// <summary>
    ///     Internal plugin version
    /// </summary>
    public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
    
    /// <summary>
    ///     Entry point for the plugin
    /// </summary>
    public Plugin(IChatGui chatGui, IClientState clientState, ICommandManager commandManager, IDalamudPluginInterface pluginInterface, IDataManager dataManager, IFramework framework, INamePlateGui namePlateGui, IObjectTable objectTable, IPluginLog pluginLog, IPlayerState playerState)
    {
        _host = new HostBuilder()
            .UseContentRoot(pluginInterface.ConfigDirectory.FullName)
            .ConfigureLogging(lb =>
            {
                lb.ClearProviders();
                lb.SetMinimumLevel(LogLevel.Trace);
            })
            .ConfigureServices(collection =>
            {
                // Base Services
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
                collection.AddSingleton(playerState);
                
                // Internal Services
                collection.AddSingleton<ConfigurationService>();
                collection.AddSingleton<FontService>();
                collection.AddSingleton<NomenclatureService>();
                collection.AddSingleton<PairService>();
                collection.AddSingleton<WorldService>();
                collection.AddSingleton<NetworkService>();
                collection.AddSingleton<NetworkRegisterService>();
                
                // Managers
                collection.AddSingleton<ConnectionManager>();
                collection.AddSingleton<LoginManager>();
                
                // Handlers
                collection.AddSingleton<ChatBoxHandler>();
                collection.AddSingleton<CommandHandler>();
                collection.AddSingleton<NamePlateHandler>();
                collection.AddSingleton<NetworkHandler>();
                
                // Network Handlers
                collection.AddSingleton<RemoveNomenclatureHandler>();
                collection.AddSingleton<UpdateNomenclatureHandler>();
                collection.AddSingleton<UpdateOnlineStatusHandler>();
                
                // IPC
                // TODO: IPCs
                
                // UI
                collection = AddUiServices(collection);

                // Startup Services
                collection.AddHostedService(p => p.GetRequiredService<ConfigurationService>());
                collection.AddHostedService(p => p.GetRequiredService<FontService>());
                collection.AddHostedService(p => p.GetRequiredService<NetworkService>());
                collection.AddHostedService(p => p.GetRequiredService<PairService>());
                
                // Startup Managers
                collection.AddHostedService(p => p.GetRequiredService<ConnectionManager>());
                collection.AddHostedService(p => p.GetRequiredService<LoginManager>());
                
                // Startup Handlers
                collection.AddHostedService(p => p.GetRequiredService<ChatBoxHandler>());
                collection.AddHostedService(p => p.GetRequiredService<CommandHandler>());
                collection.AddHostedService(p => p.GetRequiredService<NamePlateHandler>());
                collection.AddHostedService(p => p.GetRequiredService<NetworkHandler>());
                
                // Startup Ui
                collection.AddHostedService(p => p.GetRequiredService<LoginViewController>());
                collection.AddHostedService(p => p.GetRequiredService<WindowContainer>());
            }).Build();

        _ = _host.StartAsync();
    }
    
    private static IServiceCollection AddUiServices(IServiceCollection collection)
    {
        // View Controllers
        collection.AddSingleton<LoginViewController>();
        collection.AddSingleton<ManageViewController>();
        collection.AddSingleton<NomenclatureViewController>();
        collection.AddSingleton<PairsViewController>();
        collection.AddSingleton<RegisterViewController>();
        collection.AddSingleton<SettingsViewController>();
        
        // Views
        collection.AddSingleton<LoginView>();
        collection.AddSingleton<ManageView>();
        collection.AddSingleton<NomenclatureView>();
        collection.AddSingleton<PairsView>();
        collection.AddSingleton<RegisterView>();
        collection.AddSingleton<SettingsView>();
        
        // Window Controllers
        collection.AddSingleton<PrimaryWindowController>();
        
        // Windows
        collection.AddSingleton<PrimaryWindow>();
        
        // Window Container
        collection.AddSingleton<WindowContainer>();
        return collection;
    }
    
    public void Dispose()
    {
        _host.StopAsync().GetAwaiter().GetResult();
        _host.Dispose();
    }
}