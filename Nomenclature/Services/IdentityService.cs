using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Gui.NamePlate;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;

namespace Nomenclature.Services;

public class IdentityService : IHostedService
{
    private readonly INamePlateGui NamePlateGui;
    // Instantiated
    private readonly StringBuilder _handlerNameBuilder = new();

    /// <summary>
    ///     Maps [CharacterName]@[HomeWorld] to [ModifiedCharacterName]
    /// </summary>
    public Dictionary<string, string> Identities = new();

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
            
            _handlerNameBuilder.Clear();
            _handlerNameBuilder.Append(handler.Name);
            _handlerNameBuilder.Append('@');
            _handlerNameBuilder.Append(handler.PlayerCharacter.HomeWorld.Value.Name);
            if (Identities.TryGetValue(_handlerNameBuilder.ToString(), out var identity))
                handler.Name = identity;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        NamePlateGui.OnDataUpdate -= NamePlateGuiOnOnDataUpdate;

        return Task.CompletedTask;
    }
}