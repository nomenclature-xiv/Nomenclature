using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Gui.NamePlate;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureCommon.Domain;

namespace Nomenclature.Services;

public class IdentityService : IHostedService
{
    private readonly INamePlateGui NamePlateGui;

    /// <summary>
    ///     Maps [CharacterName]@[HomeWorld] to [ModifiedCharacterName]
    /// </summary>
    public Dictionary<Character, Character> Identities = new();

    public IdentityService(INamePlateGui namePlateGui)
    {
        NamePlateGui = namePlateGui;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        NamePlateGui.OnNamePlateUpdate += NamePlateGuiOnOnDataUpdate;
        return Task.CompletedTask;
    }

    private void NamePlateGuiOnOnDataUpdate(INamePlateUpdateContext context, IReadOnlyList<INamePlateUpdateHandler> handlers)
    {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < handlers.Count; i++)
        {
            var handler = handlers[i];
            if (handler.NamePlateKind is not NamePlateKind.PlayerCharacter)
                continue;

            if (handler.PlayerCharacter is null)
                continue;
            
            var character = new Character(handler.Name.ToString(), handler.PlayerCharacter.HomeWorld.Value.Name.ToString());

            if (Identities.TryGetValue(character, out var identity))
            {
                // TODO: Update world? Is this apart of nameplates?
                handler.Name = identity.Name;
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        NamePlateGui.OnDataUpdate -= NamePlateGuiOnOnDataUpdate;

        return Task.CompletedTask;
    }
}