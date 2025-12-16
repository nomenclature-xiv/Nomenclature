using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Network;
using NomenclatureClient.Services;

namespace NomenclatureClient.Managers;

public class LoginManager(IClientState client, IFramework framework, IObjectTable objectTable, ConfigurationService configuration, NetworkService network) : IHostedService
{
    /// <summary>
    ///     If we have finished processing all the login events
    /// </summary>
    public event Action? LoginFinished;
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        client.Login += OnLogin;
        client.Logout += OnLogout;
        
        if (client.IsLoggedIn)
            OnLogin();
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        client.Login -= OnLogin;
        client.Logout -= OnLogout;
        return Task.CompletedTask;
    }
    
    private async void OnLogin()
    {
        try
        {
            // This should never happen as the event we're listening to suggests the local player is available
            if (await framework.RunOnFrameworkThread(() => objectTable.LocalPlayer)  is not { } player)
                return;
            
            // Extract name and world
            var name = player.Name.ToString();
            var world = player.HomeWorld.Value.Name.ToString();
            
            // Load the configuration for this character
            await configuration.LoadCharacterConfigurationAsync(name, world).ConfigureAwait(false);
            
            // If the configuration for the character didn't load
            if (configuration.CharacterConfiguration is null)
                return;
            
            // Emit an event for the loaded login event
            LoginFinished?.Invoke();
            
            // If the character is enabled for automatic connection
            if (configuration.CharacterConfiguration.AutoConnect)
                await network.Connect();
        }
        catch (Exception)
        {
            // Ignored
        }
    }

    private void OnLogout(int type, int code)
    {
        // TODO: Clear any locally saved titles
        // <Code goes here>
        
        // Clear character configuration
        configuration.ResetCharacterConfiguration();
    }
}