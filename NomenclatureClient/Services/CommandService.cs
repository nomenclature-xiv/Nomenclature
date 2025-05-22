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
using NomenclatureClient.UI.New;
using System;
using NomenclatureClient.Services.New;
using NomenclatureCommon.Domain;
using NomenclatureClient.Debug;

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
        private readonly IpcWindow _ipcWindow;

        public CommandService(ICommandManager commandManager, IPluginLog pluginLog, MainWindow mainWindow, Configuration configuration, NetworkNameService nameService, IpcWindow ipcWindow)
        {
            CommandManager = commandManager;
            MainWindow = mainWindow;
            _nameService = nameService;
            _configuration = configuration;
            _pluginLog = pluginLog;
            _ipcWindow = ipcWindow;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "TODO"
            });
            return Task.CompletedTask;
        }

        private async void OnCommand(string command, string arguments)
        {
            if (arguments == string.Empty)
            {
                MainWindow.Toggle();
                return;
            }
            var argv = arguments.Split(' ');
            if (argv[0] == "set" && argv.Length > 2)
            {
                if (argv[1] == "name")
                {
                    var joinedname = string.Join(' ', argv.Skip(2));
                    if (await _nameService.UpdateName(joinedname, null))
                        IdentityService.CurrentNomenclature = new Nomenclature(joinedname, IdentityService.CurrentNomenclature?.World);
                }
                if (argv[1] == "world")
                {
                    var joinedworld = string.Join(' ', argv.Skip(2));
                    if(await _nameService.UpdateName(null, argv[2]))
                        IdentityService.CurrentNomenclature = new Nomenclature(IdentityService.CurrentNomenclature?.Name, joinedworld);
                }
            }
            else if (argv[0] == "debug")
            {
                _ipcWindow.Toggle();
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