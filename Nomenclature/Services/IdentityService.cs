using System;
using System.Collections.Generic;
using System.Text;
using Dalamud.Game.Gui.NamePlate;

namespace Nomenclature.Services;

public class IdentityService : IDisposable
{
    // Instantiated
    private readonly StringBuilder _handlerNameBuilder = new();

    /// <summary>
    ///     Maps [CharacterName]@[HomeWorld] to [ModifiedCharacterName]
    /// </summary>
    public readonly Dictionary<string, string> Identities = new();

    public IdentityService()
    {
        Plugin.NamePlateGui.OnNamePlateUpdate += NamePlateGuiOnOnDataUpdate;
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

    public void Dispose()
    {
        Plugin.NamePlateGui.OnDataUpdate -= NamePlateGuiOnOnDataUpdate;
        GC.SuppressFinalize(this);
    }
}