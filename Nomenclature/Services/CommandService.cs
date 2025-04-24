using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using Nomenclature.UI;
using System.Threading;
using System.Threading.Tasks;

public class CommandService : IHostedService
{
    private const string CommandName = "/nom";
    public ICommandManager CommandManager { get; }
    public MainWindow MainWindow { get; }

    public CommandService(ICommandManager commandManager, MainWindow mainWindow)
    {
        CommandManager = commandManager;
        MainWindow = mainWindow;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "TODO"
        });
        return Task.CompletedTask;
    }

    private void OnCommand(string command, string arguments)
    {
        MainWindow.Toggle();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        CommandManager.RemoveHandler(CommandName);
        return Task.CompletedTask;
    }
}