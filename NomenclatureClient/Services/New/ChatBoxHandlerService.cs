using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureCommon.Domain;

namespace NomenclatureClient.Services.New;

public class ChatBoxHandlerService(CharacterService characterService, IChatGui chatGui, IPluginLog logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        chatGui.ChatMessage += OnChatMessage;
        return Task.CompletedTask;
    }

    private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        // Consider using `type` to filter out things?
        switch (sender.Payloads.Count)
        {
            case 1: // Self
                if (characterService.CurrentCharacter is not { } character)
                    return;
                
                if (sender.TextValue.Equals(character.Name) is false)
                    return;

                // TODO: This should have the current nomenclature too
                var identifier = string.Concat(character.Name, "@", character.World);
                if (IdentityService.Identities.TryGetValue(identifier, out var nomenclature) is false)
                    return;
                
                sender.Payloads[0] = new TextPayload(string.Concat("\"", nomenclature.Name, "\""));
                break;
            
            case 3: // Same World
                if (sender.Payloads[0] is not PlayerPayload info)
                    return;
                
                var identifier2 = string.Concat(info.PlayerName, "@", info.World.Value.Name.ExtractText());
                if (IdentityService.Identities.TryGetValue(identifier2, out var nomenclature2) is false)
                    return;
                
                if (nomenclature2.Name is not null)
                    sender.Payloads[1] = new TextPayload(string.Concat("\"", nomenclature2.Name, "\""));
                
                break;
            
            case 5: // Cross World
                if (sender.Payloads[0] is not PlayerPayload info2)
                    return;
                
                var identifier3 = string.Concat(info2.PlayerName, "@", info2.World.Value.Name.ExtractText());
                if (IdentityService.Identities.TryGetValue(identifier3, out var nomenclature3) is false)
                    return;
                
                if (nomenclature3.Name is not null)
                    sender.Payloads[1] = new TextPayload(string.Concat("\"", nomenclature3.Name, "\""));
                
                if (nomenclature3.World is not null)
                    sender.Payloads[4] = new TextPayload(string.Concat("\"", nomenclature3.World, "\""));
                
                break;
            
            default:
                return;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        chatGui.ChatMessage -= OnChatMessage;
        return Task.CompletedTask;
    }
}