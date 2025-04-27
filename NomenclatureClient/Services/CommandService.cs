using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient;
using NomenclatureClient.Network;
using NomenclatureClient.UI;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NomenclatureClient.Services
{
    public class CommandService : IHostedService
    {
        private const string CommandName = "/nom";
        public ICommandManager CommandManager { get; }
        private readonly IPluginLog _pluginLog;
        public MainWindow MainWindow { get; }
        private readonly Configuration _configuration;
        private readonly NetworkNameService _nameService;

        public CommandService(ICommandManager commandManager, IPluginLog pluginLog, MainWindow mainWindow, Configuration configuration, NetworkNameService nameService)
        {
            CommandManager = commandManager;
            MainWindow = mainWindow;
            _nameService = nameService;
            _configuration = configuration;
            _pluginLog = pluginLog;
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
            if (arguments == string.Empty)
            {
                MainWindow.Toggle();
                return;
            }
            var argv = arguments.Split(' ');
            if (argv[0] == "set" && argv.Length == 3)
            {
                if (argv[1] == "name")
                {
                    _nameService.UpdateName(argv[2], null);
                }
                if (argv[1] == "world")
                {
                    _nameService.UpdateName(null, argv[2]);
                }
            }
            else
                _pluginLog.Debug("Malformed chat command.");

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            CommandManager.RemoveHandler(CommandName);
            return Task.CompletedTask;
        }
    }
}