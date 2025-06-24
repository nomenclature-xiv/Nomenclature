using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureClient.Types;
using NomenclatureCommon.Domain;

namespace NomenclatureClient.Managers;

public class LoginManager(
    IFramework framework,
    Configuration configuration,
    NetworkService networkService,
    IClientState clientState,
    SessionService sessionService) : IHostedService
{
    private async void OnLogin()
    {
        try
        {
            // This should never happen as the event we're listening to suggests the local player is available
            if (await framework.RunOnFrameworkThread(() => clientState.LocalPlayer)  is not { } player)
                return;
            
            // Get character name and their configuration
            var character = new Character(player.Name.ToString(), player.HomeWorld.Value.Name.ToString());
            if (configuration.LocalConfigurations.TryGetValue(character.ToString(), out var value) is false)
                return;
            
            // Set the session info
            sessionService.CurrentSession = new SessionInfo(character, value);
            
            // Connect if we have it enabled
            if (value.AutoConnect)
                await networkService.Connect();
        }
        catch (Exception)
        {
            // Ignored
        }
    }

    private void OnLogout(int type, int code)
    {
        // Cleanse the identity service
        IdentityService.Identities.Clear();

        // Reset the current session to an empty one
        sessionService.CurrentSession = new SessionInfo(new Character(), new CharacterConfiguration());
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        clientState.Login += OnLogin;
        clientState.Logout += OnLogout;
        
        OnLogin();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        clientState.Login -= OnLogin;
        clientState.Logout -= OnLogout;
        return Task.CompletedTask;
    }
}