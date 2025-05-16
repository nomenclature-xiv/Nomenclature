using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Gui.NamePlate;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureCommon.Domain;

namespace NomenclatureClient.Services.New;

public class NameplateHandlerService(INamePlateGui namePlateGui, Configuration config) : IHostedService
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
            
            if (config.BlocklistCharacters.Contains(new Character(handler.Name.TextValue, handler.PlayerCharacter.HomeWorld.Value.Name.ExtractText())))
                return;
            
            var identifier = $"{handler.Name}@{handler.PlayerCharacter.HomeWorld.Value.Name}";
            if (IdentityService.Identities.TryGetValue(identifier, out var identity) is false)
                continue;
            
            var name = handler.Name;
            if (identity.Name is not null)
                name = identity.Name;

            if (identity.Name != string.Empty)
                handler.Name = new SeString(new TextPayload(string.Concat(name, "*")));
            else
                handler.Name = new SeString(new TextPayload(string.Empty));

            if (identity.World is not null)
            {
                if (identity.World == string.Empty)
                    handler.FreeCompanyTag = new SeString(new TextPayload(string.Empty));
                else
                    handler.FreeCompanyTag = new SeString(new TextPayload(string.Concat(" «", identity.World, "»")));
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        namePlateGui.OnNamePlateUpdate -= OnNamePlateUpdate;
        return Task.CompletedTask;
    }
}