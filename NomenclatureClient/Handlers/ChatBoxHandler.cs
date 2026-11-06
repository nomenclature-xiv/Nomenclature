using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Chat;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Services;
using NomenclatureClient.Types;
using NomenclatureCommon.Domain;

namespace NomenclatureClient.Handlers;

public class ChatBoxHandler(IChatGui chatGui, IPlayerState playerState, IPluginLog log, ConfigurationService configuration, NomenclatureService nomenclatures) : IHostedService
{
    private void OnChatMessage(IHandleableChatMessage  message)
    {
        switch (message.LogKind)
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
            case XivChatType.StandardEmote:
                HandleDefaults(message.LogKind, message.Sender.Payloads);
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
        if (playerState.CharacterName is not {} name || playerState.HomeWorld.Value.Name.ToString() is not { } world)
            return;
        
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
                //save player data for later matching
                name = ppayload.PlayerName;
                string tworld = ppayload.World.Value.Name.ExtractText();
                worldflag = tworld != world;
                world = tworld;
                playerflag = true;
            }

            //self doesn't have playerflag, so manually check the cases
            if (playerflag || !hasplayer)
            {
                if (characterset && !worldflag)
                {
                    //playerdata, insert after
                    if (nomenclature?.WorldBehavior is NomenclatureBehavior.OverrideOriginal)
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
                            payloads.Insert(i + 1, new TextPayload(nomenclature?.World));
                        }
                    }
                    playerflag = false;
                    characterset = false;
                }
            
                //match crossworld icon + crossworld isn't home world
                else if (payloads[i] is IconPayload && ((IconPayload)payloads[i]).Icon == BitmapFontIcon.CrossWorld && worldflag && characterset)
                {
                    //need to get world position while we still know it, reference remains the same
                    var payload = (TextPayload)payloads[i + 1];
                    if(nomenclature?.WorldBehavior is NomenclatureBehavior.DisplayNothing)
                    {
                        payloads.Remove(payloads[i]);
                    }

                    if (nomenclature?.WorldBehavior is not NomenclatureBehavior.DisplayOriginal)
                    {
                        var replacewith = nomenclature?.WorldBehavior is NomenclatureBehavior.DisplayNothing
                            ? ""
                            : nomenclature?.World;
                        //we need to parse this because sqex hates me specifically
                        var tsplit = payload.Text!.Split(" ");
                        string postfix = tsplit[0].EndsWith('.') ? "." : "";
                        tsplit[0] = string.Concat(replacewith, postfix);
                        payload.Text = string.Join(" ", tsplit);
                    }
                    worldflag = false;
                    characterset = false;
                    playerflag = false;
                    i--;
                }
                //normal packet
                else if (payloads[i] is TextPayload tpayload)
                {
                    if (nomenclatures.TryGetNomenclature(name, world) is not {} _temp)
                        return;
                    nomenclature = _temp;
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
                            if (nomenclature.NameBehavior is NomenclatureBehavior.OverrideOriginal)
                            {
                                tpayload.Text = string.Concat(prefix, nomenclature.Name);
                            }
                            if (nomenclature.NameBehavior is NomenclatureBehavior.DisplayNothing)
                            {
                                tpayload.Text = prefix;
                            }
                            tpayload.Text = string.Concat(tpayload.Text, "*");
                            if (nomenclature.WorldBehavior is NomenclatureBehavior.OverrideOriginal or NomenclatureBehavior.DisplayNothing)
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
        if (characterset && nomenclature?.WorldBehavior is NomenclatureBehavior.OverrideOriginal)
        {
            payloads.Add(new IconPayload(BitmapFontIcon.CrossWorld));
            payloads.Add(new TextPayload(nomenclature?.World));
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