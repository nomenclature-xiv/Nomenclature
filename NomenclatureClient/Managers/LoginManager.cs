using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Network;
using NomenclatureClient.Services;

// ReSharper disable RedundantBoolCompare

namespace NomenclatureClient.Managers;

/// <summary>
///     Handles logging in and out of a character in game
/// </summary>
public class LoginManager : IHostedService
{
    // Injected
    private readonly IClientState _client;
    private readonly IFramework _framework;
    private readonly IObjectTable _objectTable;
    private readonly ConfigurationService _configuration;
    private readonly NetworkService _network;
    
    /// <summary>
    ///     If we have finished processing all the login events
    /// </summary>
    public event Action? LoginFinished;

    /// <summary>
    ///     <inheritdoc cref="LoginManager"/>
    /// </summary>
    public LoginManager(IClientState client, IFramework framework, IObjectTable objectTable, ConfigurationService configuration, NetworkService network)
    {
        _client = client;
        _framework = framework;
        _objectTable = objectTable;
        _configuration = configuration;
        _network = network;
        
        _client.Login += OnLogin;
        _client.Logout += OnLogout;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_client.IsLoggedIn)
            OnLogin();
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _client.Login -= OnLogin;
        _client.Logout -= OnLogout;
        return Task.CompletedTask;
    }
    
    private async void OnLogin()
    {
        try
        {
            // This should never happen as the event we're listening to suggests the local player is available
            if (await _framework.RunOnFrameworkThread(() => _objectTable.LocalPlayer).ConfigureAwait(false)  is not { } player)
                return;
            
            // Extract name and world
            var name = player.Name.ToString();
            var world = player.HomeWorld.Value.Name.ToString();
            
            // Load the configuration for this character
            if (await _configuration.LoadCharacterConfigurationAsync(name, world).ConfigureAwait(false) is false)
                return;
            
            // Emit an event for the loaded login event
            LoginFinished?.Invoke();
            
            // If the character is enabled for automatic connection
            if (_configuration.CharacterConfiguration?.AutoConnect ?? false)
                await _network.Connect().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Ignored
        }
    }

    private void OnLogout(int type, int code)
    {
        // Clear character configuration
        _configuration.ResetCharacterConfiguration();
    }
}