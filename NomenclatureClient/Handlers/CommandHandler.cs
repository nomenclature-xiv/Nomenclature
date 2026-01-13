using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Managers;
using NomenclatureClient.Services;
using NomenclatureClient.Types.Configurations;
using NomenclatureClient.UI;
using NomenclatureClient.Utils;
using NomenclatureCommon.Domain;

// ReSharper disable StringLiteralTypo

namespace NomenclatureClient.Handlers;

public partial class CommandHandler(
    IChatGui chatGui,
    ICommandManager commandManager, 
    IPluginLog logger, 
    PrimaryWindow primaryWindow,
    ConfigurationService configuration,
    NomenclatureManager nomenclatures) : IHostedService
{
    private const string CommandName = "/nom";
    private const NomenclatureBehavior Original = NomenclatureBehavior.DisplayOriginal;
    private const NomenclatureBehavior Override = NomenclatureBehavior.OverrideOriginal;
    private const NomenclatureBehavior Hide = NomenclatureBehavior.DisplayNothing;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = 
                """
                Toggles the primary plugin window
                /nom setboth "<name>" "<world>" - Sets your name and world. You must include the quotes in the command.
                /nom setname <name> - Sets your name on your nameplate
                /nom setworld <world> - Sets your world on your nameplate

                /nom hide - Hides your name and world. You must disable your title via other plugin or using in-game methods
                /nom hidename - Hides your name from your nameplate
                /nom hideworld - Hides your world from your nameplate

                /nom clear - Removes any changes to your nameplate
                /nom clearname - Removes any changes to your nameplate's name
                /nom clearworld - Removes any changes to your nameplate's world

                /nom debug - Debug command
                """
        });
        return Task.CompletedTask;
    }
    
    private void OnCommand(string command, string arguments)
    {
        if (arguments.Length is 0)
        {
            primaryWindow.Toggle();
            return;
        }

        if (configuration.CharacterConfiguration is not { } character)
        {
            logger.Warning($"[CommandHandler] Character configuration is not set, aborting command {command}");
            return;
        }
        
        var split = arguments.Split(' ');
        switch (split[0])
        {
            case "set": HandleSet(character, arguments); break;
            case "setname": HandleSetName(character, split); break;
            case "setworld": HandleSetWorld(character, split); break;
            case "hide":
                chatGui.Print(NomenclatureSeStrings.HideSuccess);
                nomenclatures.Set(character.Name, character.World, string.Empty, Hide, string.Empty, Hide);
                break;
            
            case "hidename":
                chatGui.Print(NomenclatureSeStrings.HideNameSuccess);
                nomenclatures.SetName(character.Name, character.World, string.Empty, Hide);
                break;
            
            case "hideworld":
                chatGui.Print(NomenclatureSeStrings.HideWorldSuccess);
                nomenclatures.SetWorld(character.Name, character.World, string.Empty, Hide);
                break;
            
            case "clear":
                chatGui.Print(NomenclatureSeStrings.ClearSuccess);
                nomenclatures.Set(character.Name, character.World, string.Empty, Original, string.Empty, Original);
                break;
            
            case "clearname":
                chatGui.Print(NomenclatureSeStrings.ClearNameSuccess);
                nomenclatures.SetName(character.Name, character.World, string.Empty, Original);
                break;
            
            case "clearworld":
                chatGui.Print(NomenclatureSeStrings.ClearWorldSuccess);
                nomenclatures.SetWorld(character.Name, character.World, string.Empty, Original);
                break;
            
            case "debug":
                // Implement something here if needed
                break;
            
            default:
                logger.Warning($"[CommandHandler] Unknown command /nom {arguments}");
                break;
        }
    }
    
    private void HandleSet(CharacterConfigurationV2 character, string arguments)
    {
        var matches = TextWithinNonEscapedQuotes().Matches(arguments);
        if (matches.Count is not 2)
        {
            chatGui.Print(NomenclatureSeStrings.SetError);
            return;
        }
                
        var name = Regex.Unescape(matches[0].Groups[1].Value);
        var world = Regex.Unescape(matches[1].Groups[1].Value);
                
        // TODO: Validation on length, special characters, etc.
        
        nomenclatures.Set(character.Name, character.World, name, Override, world, Override);
        chatGui.Print(NomenclatureSeStrings.SetSuccess);
    }

    private void HandleSetName(CharacterConfigurationV2 character, string[] split)
    {
        if (split.Length < 2)
        {
            chatGui.Print(NomenclatureSeStrings.SetNameError);
            return;
        }
                
        var name = string.Join(" ", split[1..]);
        
        // TODO: Validation on length, special characters, etc.
        
        nomenclatures.SetName(character.Name, character.World, name, Override);
        chatGui.Print(NomenclatureSeStrings.SetNameSuccess);
    }

    private void HandleSetWorld(CharacterConfigurationV2 character, string[] split)
    {
        if (split.Length < 2)
        {
            chatGui.Print(NomenclatureSeStrings.SetWorldError);
            return;
        }
                
        var name = string.Join(" ", split[1..]);
        
        // TODO: Validation on length, special characters, etc.
        
        nomenclatures.SetWorld(character.Name, character.World, name, Override);
        chatGui.Print(NomenclatureSeStrings.SetWorldSuccess);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        commandManager.RemoveHandler(CommandName);
        return Task.CompletedTask;
    }

    [GeneratedRegex("\"((?:\\\\.|[^\"\\\\])*)\"")]
    private static partial Regex TextWithinNonEscapedQuotes();
}