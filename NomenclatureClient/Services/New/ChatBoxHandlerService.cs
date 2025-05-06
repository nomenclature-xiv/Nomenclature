using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureCommon.Domain;
using static FFXIVClientStructs.FFXIV.Client.System.String.Utf8String.Delegates;

namespace NomenclatureClient.Services.New;

public class ChatBoxHandlerService(CharacterService characterService, Configuration configuration, IChatGui chatGui, IPluginLog logger) : IHostedService
{
    private readonly IconPayload cwpayload = new IconPayload(BitmapFontIcon.CrossWorld);
    public Task StartAsync(CancellationToken cancellationToken)
    {
        chatGui.ChatMessage += OnChatMessage;
        return Task.CompletedTask;
    }

    private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (characterService.CurrentCharacter is not { } character)
            return;
        // Consider using `type` to filter out things?
        switch (sender.Payloads.Count)
        {
            case 1: // Self
                if (sender.TextValue.Equals(character.Name) is false)
                    return;

                // TODO: This should have the current nomenclature too
                var identifier = string.Concat(character.Name, "@", character.World);
                if (IdentityService.Identities.TryGetValue(identifier, out var nomenclature) is false)
                    return;

                if (nomenclature.Name is not null)
                    sender.Payloads[0] = new TextPayload(string.Concat("\"", nomenclature.Name, "\""));
                if (nomenclature.World is not null)
                {
                    if (HandleWorld(nomenclature.World, characterService.CurrentCharacter) is { } payload)
                    {
                        sender.Payloads.Add(cwpayload);
                        sender.Payloads.Add(payload);
                    }
                }

                break;
            
            case 3: // Same World
                if (sender.Payloads[0] is not PlayerPayload info)
                    return;

                if (configuration.BlocklistCharacters.Contains(new Character(info.PlayerName, info.World.Value.Name.ExtractText())))
                    return;
                var identifier2 = string.Concat(info.PlayerName, "@", info.World.Value.Name.ExtractText());
                if (IdentityService.Identities.TryGetValue(identifier2, out var nomenclature2) is false)
                    return;
                
                if (nomenclature2.Name is not null)
                    sender.Payloads[1] = new TextPayload(string.Concat("\"", nomenclature2.Name, "\""));
                if (nomenclature2.World is not null)
                {
                    if (HandleWorld(nomenclature2.World, characterService.CurrentCharacter) is { } payload)
                    {
                        sender.Payloads.Add(cwpayload);
                        sender.Payloads.Add(payload);
                    }
                }
                break;
            
            case 5: // Cross World
                if (sender.Payloads[0] is not PlayerPayload info2)
                    return;
                if (configuration.BlocklistCharacters.Contains(new Character(info2.PlayerName, info2.World.Value.Name.ExtractText())))
                    return;
                var identifier3 = string.Concat(info2.PlayerName, "@", info2.World.Value.Name.ExtractText());
                if (IdentityService.Identities.TryGetValue(identifier3, out var nomenclature3) is false)
                    return;
                
                if (nomenclature3.Name is not null)
                    sender.Payloads[1] = new TextPayload(string.Concat("\"", nomenclature3.Name, "\""));

                if (nomenclature3.World is not null)
                {
                    if (HandleWorld(nomenclature3.World, characterService.CurrentCharacter) is { } payload)
                    {
                        sender.Payloads[4] = payload;
                    }
                }

                break;
            
            default:
                return;
        }
    }

    private TextPayload? HandleWorld(string world, Character self)
    {
        if(world == self.World)
        {
            return null;
        }
        return new TextPayload(world);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        chatGui.ChatMessage -= OnChatMessage;
        return Task.CompletedTask;
    }
}