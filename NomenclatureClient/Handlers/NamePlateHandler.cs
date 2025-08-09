using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Gui.NamePlate;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Services;

namespace NomenclatureClient.Handlers;

public class NamePlateHandler(INamePlateGui namePlateGui) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        namePlateGui.OnNamePlateUpdate += OnNamePlateUpdate;
        return Task.CompletedTask;
    }

    private static void OnNamePlateUpdate(INamePlateUpdateContext context, IReadOnlyList<INamePlateUpdateHandler> handlers)
    {
        foreach (var handler in handlers)
        {
            if (handler.NamePlateKind is not NamePlateKind.PlayerCharacter)
                continue;
            
            if (handler.PlayerCharacter is null)
                continue;
            
            var identifier = $"{handler.Name}@{handler.PlayerCharacter.HomeWorld.Value.Name}";
            if (IdentityService.Identities.TryGetValue(identifier, out var identity) is false)
                continue;
            
            var name = handler.Name;
            if (identity.Name is not null)
                name = identity.Name;

            handler.Name = identity.Name == string.Empty
                ? new SeString(new TextPayload(string.Empty))
                : new SeString(new TextPayload(string.Concat(name, "*")));

            if (identity.World is null)
                continue;
            
            handler.FreeCompanyTag = identity.World == string.Empty 
                ? new SeString(new TextPayload(string.Empty)) 
                : new SeString(new TextPayload(string.Concat(" «", identity.World, "»")));

        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        namePlateGui.OnNamePlateUpdate -= OnNamePlateUpdate;
        return Task.CompletedTask;
    }
}