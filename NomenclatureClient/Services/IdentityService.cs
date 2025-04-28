using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Gui.NamePlate;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Utils;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Api.Server;

namespace NomenclatureClient.Services;

public class IdentityService : IHostedService
{
    private readonly IconPayload cwpayload = new IconPayload(BitmapFontIcon.CrossWorld);

    private readonly INamePlateGui NamePlateGui;
    private readonly IChatGui ChatGui;
    private readonly IPluginLog PluginLog;
    private readonly CharacterService CharacterService;
    // Instantiated
    private readonly StringBuilder _handlerNameBuilder = new();

    /// <summary>
    ///     Maps [CharacterName]@[HomeWorld] to [ModifiedCharacterName]
    /// </summary>
    public Dictionary<Character, Nomenclature> Identities = new();

    public IdentityService(INamePlateGui namePlateGui, IChatGui chatGui, IPluginLog pluginLog, CharacterService characterService)
    {
        NamePlateGui = namePlateGui;
        ChatGui = chatGui;
        CharacterService = characterService;
        PluginLog = pluginLog;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        NamePlateGui.OnNamePlateUpdate += NamePlateGuiOnOnDataUpdate;
        ChatGui.ChatMessageHandled += ChatGuiOnMessageHandled;
        ChatGui.ChatMessage += ChatGuiOnChatMessage;
        return Task.CompletedTask;
    }

    private void ChatGuiOnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        var payloads = sender.Payloads;
        var self = CharacterService.CurrentCharacter;
        if (self is null)
            return;
        if (payloads.Count is 1)
        {
            //it's you!
            ChangeName(ref payloads, self, self);
        }
        else if(payloads.Count is 3)
        {
            string sendername = ((TextPayload)payloads[1]).Text;
            string icon = "";
            if (!char.IsUpper(sendername[0]))
            {
                icon = sendername.Substring(0, 1);
                sendername = sendername.Substring(1);
            }
            Character senderchar = new Character(sendername, self.World);
            Payload player = payloads[0];
            Payload id = payloads[2];
            ChangeName(ref payloads, senderchar, self, icon);
            payloads.Insert(0, player);
            payloads.Insert(2, id);
        }
        else if(payloads.Count is 5)
        {
            //crossworld
            string sendername = ((TextPayload)payloads[1]).Text;
            string icon = "";
            if(!char.IsUpper(sendername[0]))
            {
                icon = sendername.Substring(0, 1);
                sendername = sendername.Substring(1);
            }
            Character senderchar = new Character(sendername, ((TextPayload)payloads[4]).Text);
            Payload player = payloads[0];
            Payload id = payloads[2];
            ChangeName(ref payloads, senderchar, self, icon);
            payloads.Insert(0, player);
            payloads.Insert(2, id);
        }
    }

    private void ChatGuiOnMessageHandled(XivChatType type, int timestamp, SeString sender, SeString message)
    {
        var test = sender.Payloads;
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
                if(identity.Name is null)
                {
                    continue;
                }
                if (identity.Name == string.Empty)
                    handler.Name = string.Empty;
                else
                {
                    handler.Name = $"\"{identity.Name}\"";
                }
                //«{identity.World}»
            }
        }
    }

    private void ChangeName(ref List<Payload> payloads, Character character, Character self, string icon = "")
    {
        if (Identities.ContainsKey(character))
        {
            var changedname = Identities[character];
            payloads.Clear();
            PluginLog.Debug($"{changedname.World is null}");
            if(changedname.Name is null)
            {
                changedname = new Nomenclature(character.Name, changedname.World);
            }
            if (changedname.World is null)
            {
                changedname = new Nomenclature(changedname.Name, character.World);
            }
            if (changedname.World != self.World)
            {
                PluginLog.Debug($"{changedname.Name} {changedname.World}");
                //modified world, show as crossworld!
                payloads.Add(new TextPayload($"{icon}\"{changedname.Name}\""));
                payloads.Add(cwpayload);
                payloads.Add(new TextPayload(changedname.World));
            }
            else
            {
                //same world, just modify name
                payloads.Add(new TextPayload($"\"{changedname.Name}\""));
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        NamePlateGui.OnDataUpdate -= NamePlateGuiOnOnDataUpdate;
        ChatGui.ChatMessageHandled -= ChatGuiOnMessageHandled;

        return Task.CompletedTask;
    }
}