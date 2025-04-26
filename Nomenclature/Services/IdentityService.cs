using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Gui.NamePlate;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using Nomenclature.Utils;
using NomenclatureCommon.Domain;

namespace Nomenclature.Services;

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
    public Dictionary<Character, Character> Identities = new();

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
        if(payloads.Count is 1)
        {
            //it's you!
            var selftask = CharacterService.GetCurrentCharacter();
            selftask.Wait();
            var self = selftask.Result;
            if(self is null)
            {
                return;
            }
            /*
            if(Identities.ContainsKey(name))
            {
                var changedname = Identities[name];
                var charworld = NameConvert.ToTuple(changedname);
                payloads.Clear();
                if(charworld.Item2 != self?.WorldName)
                {
                    //modified world, show as crossworld!
                    payloads.Add(new TextPayload(charworld.Item1));
                    payloads.Add(cwpayload);
                    payloads.Add(new TextPayload(charworld.Item2));
                }
                else
                {
                    //same world, just modify name
                    payloads.Add(new TextPayload(charworld.Item1));
                }
            }*/
            if (Identities.TryGetValue(self, out var identity))
            {
                payloads[0] = new TextPayload(identity.Name);
                
            }
        }
        if(payloads.Count is 3)
        {
            //same world
        }
        if(payloads.Count is 5)
        {
            //crossworld
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
                // TODO: Update world? Is this apart of nameplates?
                handler.Name = identity.Name;
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