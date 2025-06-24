using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Debug;
using NomenclatureClient.Managers;
using NomenclatureClient.UI;

namespace NomenclatureClient.Handlers;

public class CommandHandler(
    ICommandManager commandManager,
    MainWindow mainWindow,
    IdentityManager identityManager,
    IpcWindow ipcWindow) : IHostedService
{
    private const string CommandName = "/nom";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "TODO"
        });
        return Task.CompletedTask;
    }

    private async void OnCommand(string command, string arguments)
    {
        try
        {
            if (arguments == string.Empty)
            {
                mainWindow.Toggle();
                return;
            }

            var args = arguments.Split(' ');
            var subcommand = args[0];  // Example: "/nom set" makes this subcommand equal "set"

            switch (subcommand)
            {
                case SubCommand.Set:
                    if (args.Length <= 1) // Example: "/nom set" means the size of this is 1
                        return;

                    var setArguments = args[1]; // Example: "/nom set name doughnut woman" means this is "name"
                    switch (setArguments)
                    {
                        case SubCommand.SubArgument.Name:
                            var name = string.Join(" ", args.Skip(2));
                            await identityManager.SetName(name);
                            break;

                        case SubCommand.SubArgument.World:
                            var world = string.Join(" ", args.Skip(2));
                            await identityManager.SetWorld(world);
                            break;
                    }

                    break;
                
                case SubCommand.Clear:
                case SubCommand.Reset:
                    if (args.Length <= 1)
                    {
                        await identityManager.ClearNameAndWorld();
                        return;
                    }
                    
                    var resetArguments = args[1];
                    switch (resetArguments)
                    {
                        case SubCommand.SubArgument.Name:
                            await identityManager.ClearName();
                            break;
                        
                        case SubCommand.SubArgument.World:
                            await identityManager.ClearWorld();
                            break;
                    }
                    
                    break;

                case SubCommand.Debug:
                    ipcWindow.Toggle();
                    break;
            }
        }
        catch (Exception)
        {
            // Ignore
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        commandManager.RemoveHandler(CommandName);
        return Task.CompletedTask;
    }

    private static class SubCommand
    {
        public const string Set = "set";
        public const string Reset = "reset";
        public const string Clear = "clear";
        public const string Debug = "debug";

        public static class SubArgument
        {
            public const string Name = "name";
            public const string World = "world";
        }
    }
}