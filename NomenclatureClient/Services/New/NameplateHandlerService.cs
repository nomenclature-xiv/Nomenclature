using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Gui.NamePlate;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace NomenclatureClient.Services.New;

public class NameplateHandlerService(INamePlateGui namePlateGui, IPluginLog logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        namePlateGui.OnNamePlateUpdate += OnNamePlateUpdate;
        return Task.CompletedTask;
    }

    private void OnNamePlateUpdate(INamePlateUpdateContext context, IReadOnlyList<INamePlateUpdateHandler> handlers)
    {
        foreach (var handler in handlers)
        {
            if (handler.NamePlateKind is not NamePlateKind.PlayerCharacter)
                continue;
            
            if (handler.PlayerCharacter is null)
                continue;
            
            var identifier = $"{handler.Name}@{handler.PlayerCharacter.HomeWorld.Value.Name}";
            logger.Info(identifier);
            if (IdentityService.Identities.TryGetValue(identifier, out var identity) is false)
                continue;

            if (identity.Name is not null)
                handler.Name = new SeString(new TextPayload(string.Concat("\"", identity.Name, "\"")));
            
            if (identity.World is not null)
                handler.FreeCompanyTag = new SeString(new TextPayload(string.Concat(" «", identity.World, "»")));
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        namePlateGui.OnNamePlateUpdate -= OnNamePlateUpdate;
        return Task.CompletedTask;
    }
}