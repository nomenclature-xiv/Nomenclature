using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Gui.NamePlate;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Managers;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain;

namespace NomenclatureClient.Handlers;

public class NamePlateHandler(INamePlateGui namePlateGui, NomenclatureService nomenclatures) : IHostedService
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
            if (handler.NamePlateKind is not NamePlateKind.PlayerCharacter) continue;
            if (handler.PlayerCharacter is null) continue;

            if (nomenclatures.TryGetNomenclature(handler.Name.ToString(), handler.PlayerCharacter.HomeWorld.Value.Name.ToString()) is not { } nomenclature)
                continue;

            switch (nomenclature.NameBehavior)
            {
                case NomenclatureBehavior.OverrideOriginal:
                    handler.Name = new SeString(new TextPayload(string.Concat(nomenclature.Name, "♪")));
                    break;
                
                case NomenclatureBehavior.DisplayNothing:
                    handler.NameIconId = -1;
                    handler.Name = new SeString(new TextPayload(string.Empty));
                    break;
                
                case NomenclatureBehavior.DisplayOriginal:
                default:
                    break;
            }
            
            switch (nomenclature.WorldBehavior)
            {
                case NomenclatureBehavior.OverrideOriginal:
                    handler.FreeCompanyTag = new SeString(new TextPayload(string.Concat(" «", nomenclature.World, "»")));
                    break;
                
                case NomenclatureBehavior.DisplayNothing:
                    handler.FreeCompanyTag = new SeString(new TextPayload(string.Empty));
                    break;
                
                case NomenclatureBehavior.DisplayOriginal:
                default:
                    break;
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        namePlateGui.OnNamePlateUpdate -= OnNamePlateUpdate;
        return Task.CompletedTask;
    }
}