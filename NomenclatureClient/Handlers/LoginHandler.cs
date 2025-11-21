using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Managers;
using NomenclatureClient.Services;
using NomenclatureClient.Types;
using NomenclatureClient.UI;
using NomenclatureCommon.Domain;

namespace NomenclatureClient.Handlers;

public class LoginHandler(
    IPluginLog logger,
    IFramework framework,
    Configuration configuration,
    IClientState clientState,
    SessionService sessionService,
    MainWindowController controller,
    IdentityManager identityManager) : IHostedService
{
    private async void OnLogin()
    {
        try
        {
            // This should never happen as the event we're listening to suggests the local player is available
            if (await framework.RunOnFrameworkThread(() => clientState.LocalPlayer) is not { } player)
                return;
            
            // Get character name and their configuration
            var character = new Character(player.Name.ToString(), player.HomeWorld.Value.Name.ToString());
            if (configuration.LocalConfigurations.TryGetValue(character.ToString(), out var value) is false)
            {
                value = new CharacterConfiguration();
                configuration.LocalConfigurations.Add(character.ToString(), value);
            }
            
            // Set the session info
            sessionService.CurrentSession = new SessionInfo(character, value);

            controller.OverrideName = value.OverrideName;
            controller.OverrideWorld = value.OverrideWorld;
            controller.OverwrittenName = value.Name ?? "";
            controller.OverwrittenWorld = value.World ?? "";

            identityManager.SetConfig(value);
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