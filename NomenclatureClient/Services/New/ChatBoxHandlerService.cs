using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureCommon.Domain;
using static FFXIVClientStructs.FFXIV.Client.LayoutEngine.ILayoutInstance;
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

        string name = character.Name;
        string world = character.World;
        bool characterset = false;
        int worldindex = -1;
        Nomenclature? nomenclature = null;

        // Consider using `type` to filter out things?
        foreach(Payload payload in sender.Payloads)
        {
            if(payload is PlayerPayload ppayload)
            {
                name = ppayload.PlayerName;
                world = ppayload.World.Value.Name.ExtractText();
            }

            if(payload is TextPayload tpayload)
            {
                if (configuration.BlocklistCharacters.Contains(new Character(name, world)))
                    return;
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
                            tpayload.Text = string.Concat("\"", prefix, nomenclature.Name, "\"");
                        }
                        characterset = true;
                    }
                }
                else
                {
                    if(nomenclature.World is not null)
                    {
                        worldindex = sender.Payloads.IndexOf(payload);
                        tpayload.Text = nomenclature.World;
                    }
                }
            }
        }
        if (nomenclature?.World is not null)
        {
            if (worldindex is not -1)
            {
                sender.Payloads.Insert(worldindex, new UIForegroundPayload(9));
                sender.Payloads.Insert(worldindex + 2, UIForegroundPayload.UIForegroundOff);
            }
            else
            {
                sender.Payloads.Add(cwpayload);
                sender.Payloads.Add(new UIForegroundPayload(9));
                sender.Payloads.Add(new TextPayload(nomenclature.World));
                sender.Payloads.Add(UIForegroundPayload.UIForegroundOff);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        chatGui.ChatMessage -= OnChatMessage;
        return Task.CompletedTask;
    }
}