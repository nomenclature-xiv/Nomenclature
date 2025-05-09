using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;

namespace NomenclatureClient.Services.New;

public class ChatBoxHandlerService(CharacterService characterService, Configuration configuration, IChatGui chatGui, IPluginLog logger) : IHostedService
{
    private readonly IconPayload _crossWorldIconPayload = new(BitmapFontIcon.CrossWorld);
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        chatGui.ChatMessage += OnChatMessage;
        return Task.CompletedTask;
    }

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
                HandleDefault(sender.Payloads);
                break;
            
            case XivChatType.StandardEmote:
                HandleDefault(message.Payloads);
                break;
            
            case XivChatType.Party:
            case XivChatType.CrossParty:
            case XivChatType.Alliance:
                HandleParty(sender.Payloads);
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

    // == Self ==
    // Text
            
    // == Other, Same World ==
    // Player
    // Text
    // Unknown
            
    // == Other, Different World ==
    // Player
    // Text
    // Unknown
    // Icon
    // Text
    private void HandleDefault(List<Payload> payloads)
    {
        if (payloads[0] is PlayerPayload playerPayload)
        {
            var identifier = string.Concat(playerPayload.PlayerName, "@", playerPayload.World.Value.Name.ExtractText());
            if (IdentityService.Identities.TryGetValue(identifier, out var nomenclature) is false)
                return;

            var modified = new List<Payload> { payloads[0] };
            if (nomenclature.Name is not null)
            {
                modified.Add(new EmphasisItalicPayload(true));
                modified.Add(new TextPayload(string.Concat(nomenclature.Name, "  ")));
                modified.Add(new EmphasisItalicPayload(false));
            }
            else
            {
                modified.Add(payloads[1]);
            }
            
            modified.Add(payloads[2]);

            if (nomenclature.World is not null)
            {
                modified.Add(_crossWorldIconPayload);
                modified.Add(new EmphasisItalicPayload(true));
                modified.Add(new TextPayload(nomenclature.World));
                modified.Add(new EmphasisItalicPayload(false));
            }
            else
            {
                if (payloads.Count is 5)
                {
                    modified.Add(payloads[3]);
                    modified.Add(payloads[4]);
                }
            }
            
            payloads.Clear();
            payloads.AddRange(modified);
        }
        else
        {
            if (characterService.CurrentCharacter is not { } character)
                return;
            
            var identifier = string.Concat(character.Name, "@", character.World);
            if (IdentityService.Identities.TryGetValue(identifier, out var identity) is false)
                return;

            if (identity.Name is null)
                return;
            
            payloads.Clear();
            payloads.Add(new EmphasisItalicPayload(true));
            payloads.Add(new TextPayload(identity.Name));
            payloads.Add(new EmphasisItalicPayload(false));
        }
    }
    
    // == Self ==
    // Unknown
    // Text
    // Unknown
    // Unknown
    // Text
    // Unknown
            
    // == Other, Same World ==
    // 0 Unknown
    // 1 Player
    // 2 Text
    // 3 Unknown
    // 4 Unknown
    // 5 Text
    // 6 Unknown
    // 7 Unknown
            
    // == Other, Different World ==
    // 0 Unknown
    // 1 Player
    // 2 Text
    // 3 Unknown
    // 4 Unknown
    // 5 Text
    // 6 Unknown
    // 7 Icon
    // 8 Text
    // 9 Unknown
    private void HandleParty(List<Payload> payloads)
    {
        if (payloads[1] is PlayerPayload playerPayload)
        {
            var identifier = string.Concat(playerPayload.PlayerName, "@", playerPayload.World.Value.Name.ExtractText());
            if (IdentityService.Identities.TryGetValue(identifier, out var nomenclature) is false)
                return;
            
            var modified = new List<Payload> { payloads[2], payloads[3], payloads[4] };
            if (nomenclature.Name is not null)
            {
                modified.Add(new EmphasisItalicPayload(true));
                modified.Add(new TextPayload(string.Concat(nomenclature.Name, "  ")));
                modified.Add(new EmphasisItalicPayload(false));
            }
            else
            {
                modified.Add(payloads[5]);
            }
            
            modified.Add(payloads[6]);

            if (nomenclature.World is not null)
            {
                modified.Add(_crossWorldIconPayload);
                modified.Add(new EmphasisItalicPayload(true));
                modified.Add(new TextPayload(nomenclature.World));
                modified.Add(new EmphasisItalicPayload(false));
            }
            else
            {
                if (payloads.Count is 9)
                {
                    modified.Add(payloads[7]);
                    modified.Add(payloads[8]);
                }
            }
            
            payloads.Clear();
            payloads.AddRange(modified);
        }
        else
        {
            if (characterService.CurrentCharacter is not { } character)
                return;
            
            var identifier = string.Concat(character.Name, "@", character.World);
            if (IdentityService.Identities.TryGetValue(identifier, out var identity) is false)
                return;

            if (identity.Name is null)
                return;

            var icon = payloads[1];
            payloads.Clear();
            payloads.Add(icon);
            payloads.Add(new EmphasisItalicPayload(true));
            payloads.Add(new TextPayload(identity.Name));
            payloads.Add(new EmphasisItalicPayload(false));
        }
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        chatGui.ChatMessage -= OnChatMessage;
        return Task.CompletedTask;
    }
}