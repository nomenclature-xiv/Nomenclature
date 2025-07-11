using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain;

namespace NomenclatureClient.Handlers;

public class ChatBoxHandler(IChatGui chatGui, IClientState clientState) : IHostedService
{
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
                HandleDefault(sender.Payloads);
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

    private void HandleDefault(List<Payload> payloads)
    {
        if (clientState.LocalPlayer is not { } player || player.HomeWorld.Value.Name.ToString() is not { } homeworld)
            return;
        
        var character = new Character(player.Name.ToString(), homeworld);

        string name = character.Name;
        string world = character.World;
        bool hasplayer = payloads.Where(p => p is PlayerPayload).Count() > 0;
        bool characterset = false;
        bool playerflag = false;
        bool worldflag = false;
        Nomenclature? nomenclature = null;

        // Consider using `type` to filter out things?
        for(int i = 0; i < payloads.Count; i++)
        {
            if (payloads[i] is PlayerPayload ppayload)
            {
                name = ppayload.PlayerName;
                string tworld = ppayload.World.Value.Name.ExtractText();
                worldflag = tworld != world;
                world = tworld;
                playerflag = true;
            }

            //self doesn't have playerflag, so manually check the cases
            if(playerflag || !hasplayer)
            {
                if (characterset && !worldflag)
                {
                    //playerdata, insert after
                    if (nomenclature?.World != string.Empty)
                    {
                        if (payloads[i] is RawPayload)
                        {
                            payloads.Insert(i + 1, new IconPayload(BitmapFontIcon.CrossWorld));
                            payloads.Insert(i + 2, new TextPayload(nomenclature?.World));
                            i += 2;
                        }
                        else
                        {
                            payloads.Insert(i, new IconPayload(BitmapFontIcon.CrossWorld));
                            payloads.Insert(i + 1, new TextPayload(nomenclature.World));
                        }
                    }
                    playerflag = false;
                    characterset = false;
                }

                else if (payloads[i] is IconPayload && ((IconPayload)payloads[i]).Icon == BitmapFontIcon.CrossWorld && worldflag && characterset)
                {
                    if(nomenclature?.World == string.Empty)
                    {
                        payloads.Remove(payloads[i]);
                    }
                    TextPayload tpayload = (TextPayload)payloads[i];
                    //we need to parse this because sqex hates me specifically
                    var tsplit = tpayload.Text!.Split(" ");
                    string postfix = tsplit[0].EndsWith('.') ? "." : "";
                    tsplit[0] = string.Concat(nomenclature?.World!, postfix);
                    tpayload.Text = string.Join(" ", tsplit);
                    worldflag = false;
                    characterset = false;
                    playerflag = false;
                    i--;
                }

                else if (payloads[i] is TextPayload tpayload)
                {
                    var identifier = string.Concat(name, "@", world);
                    if (IdentityService.Identities.TryGetValue(identifier, out nomenclature) is false)
                        continue;
                    
                    if (!characterset)
                    {
                        //handling for friend group icons!
                        string testtext = tpayload.Text ?? string.Empty;
                        string prefix = string.Empty;
                        if (!Char.IsUpper(testtext[0]))
                        {
                            prefix = testtext.Substring(0, 1);
                            testtext = testtext.Substring(1);
                        }
                        if (name == testtext)
                        {
                            if (nomenclature.Name is not null)
                            {
                                tpayload.Text = string.Concat(prefix, nomenclature.Name);
                            }
                            tpayload.Text = string.Concat(tpayload.Text, "*");
                            if (nomenclature.World is not null)
                            {
                                characterset = true;
                            }
                            else
                            {
                                playerflag = false;
                            }
                        }
                    }
                }
            }
        }
        if (characterset && nomenclature?.World != string.Empty)
        {
            payloads.Add(new IconPayload(BitmapFontIcon.CrossWorld));
            payloads.Add(new TextPayload(nomenclature?.World));
        }
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        chatGui.ChatMessage -= OnChatMessage;
        return Task.CompletedTask;
    }
}