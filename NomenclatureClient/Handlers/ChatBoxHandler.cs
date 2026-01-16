using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Managers;
using NomenclatureClient.Services;

namespace NomenclatureClient.Handlers;

public class ChatBoxHandler(IChatGui chatGui, ConfigurationService configuration, NomenclatureService nomenclatures) : IHostedService
{
    private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        switch (type)
        {
            case XivChatType.Say:
            case XivChatType.Shout:
            case XivChatType.TellOutgoing:
            case XivChatType.TellIncoming:
            case XivChatType.Ls1:
            case XivChatType.Ls2:
            case XivChatType.Ls3:
            case XivChatType.Ls4:
            case XivChatType.Ls5:
            case XivChatType.Ls6:
            case XivChatType.Ls7:
            case XivChatType.Ls8:
            case XivChatType.CrossLinkShell1:
            case XivChatType.CrossLinkShell2:
            case XivChatType.CrossLinkShell3:
            case XivChatType.CrossLinkShell4:
            case XivChatType.CrossLinkShell5:
            case XivChatType.CrossLinkShell6:
            case XivChatType.CrossLinkShell7:
            case XivChatType.CrossLinkShell8:
            case XivChatType.FreeCompany:
            case XivChatType.CustomEmote:
            case XivChatType.Yell:
                
            // These cases were separate in the old model
            case XivChatType.Party:
            case XivChatType.CrossParty:
            case XivChatType.Alliance:
                HandleDefaults(type, sender.Payloads);
                break;
            
            case XivChatType.StandardEmote:
                HandleDefaults(type, message.Payloads);
                break;
            
            case XivChatType.None:
            case XivChatType.Debug:
            case XivChatType.Urgent:
            case XivChatType.Notice:
            case XivChatType.Echo:
            case XivChatType.SystemError:
            case XivChatType.SystemMessage:
            case XivChatType.GatheringSystemMessage:
            case XivChatType.ErrorMessage:
            case XivChatType.NPCDialogue:
            case XivChatType.NPCDialogueAnnouncements:
            case XivChatType.RetainerSale:
            case XivChatType.NoviceNetwork:
            case XivChatType.PvPTeam:
            default:
                return;
        }
    }

    private void HandleDefaults(XivChatType type, List<Payload> payloads)
    {
        // This message doesn't contain a player packet, which means we sent it
        var self = payloads.FindIndex(payload => payload is PlayerPayload);
        if (self < 0)
        {
            if (configuration.CharacterConfiguration is null) return;
            var name = configuration.CharacterConfiguration.Name;
            var world = configuration.CharacterConfiguration.World;

            if (nomenclatures.TryGetNomenclature(name, world) is not { } nomenclature) return;
            foreach (var payload in payloads)
            {
                if (payload is not TextPayload text) continue;
                if (text.Text is null) continue;
                
                // Replace any occurrence of our name
                text.Text = text.Text.Replace(name, nomenclature.Name);
                
                // Only replace the world if it literally is the exact string
                if (text.Text.Trim() == world)
                    text.Text = nomenclature.World;
            }
        }
        else
        {
            var info = (PlayerPayload)payloads[self];
            var name = info.PlayerName;
            var world = info.World.Value.Name.ToString();

            if (nomenclatures.TryGetNomenclature(name, world) is not { } nomenclature) return;
            foreach (var payload in payloads)
            {
                if (payload is not TextPayload text) continue;
                if (text.Text is null) continue;
                
                // Replace any occurrence of our name
                text.Text = text.Text.Replace(name, nomenclature.Name);

                if (type is XivChatType.StandardEmote)
                {
                    // Emotes are a little odd in that they contain the world in the string as well...
                    if (text.Text.StartsWith(world))
                        text.Text = nomenclature.World + text.Text[world.Length..];
                }
                else
                {
                    // Only replace the world if it literally is the exact string
                    if (text.Text.Trim() == world)
                        text.Text = nomenclature.World;
                }
            }
        }
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        chatGui.ChatMessage += OnChatMessage;
        return Task.CompletedTask;
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        chatGui.ChatMessage -= OnChatMessage;
        return Task.CompletedTask;
    }
}