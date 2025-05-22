using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using System;
using NomenclatureClient.UI.New;
using Microsoft.Extensions.Hosting;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NomenclatureClient.Services.New;

namespace NomenclatureClient.Ipc
{
    public class IpcManager : IHostedService
    {
        private readonly IDalamudPluginInterface _pluginInterface;
        private readonly NetworkNameService _networkNameService;
        private readonly CharacterService _characterService;
        private readonly MainWindowController _mainWindowController;

        public const string SetNomenclature = "Nomenclature.SetNomenclature";
        public const string GetNomenclature = "Nomenclature.GetNomenclature";

        private ICallGateProvider<string, ushort, object?> _setNomenclature;
        private ICallGateProvider<string> _getNomenclature;

        public IpcManager(IDalamudPluginInterface pluginInterface, NetworkNameService nameService, CharacterService characterService, MainWindowController mainWindowController)
        {
            _pluginInterface = pluginInterface;
            _networkNameService = nameService;
            _characterService = characterService;
            _mainWindowController = mainWindowController;
            _setNomenclature = _pluginInterface.GetIpcProvider<string, ushort, object?>(SetNomenclature);
            _getNomenclature = _pluginInterface.GetIpcProvider<string>(GetNomenclature);
            AddActions();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            AddActions();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _setNomenclature.UnregisterAction();
            _getNomenclature.UnregisterAction();
            return Task.CompletedTask;
        }

        private void AddActions()
        {
            _setNomenclature.RegisterAction((nomen, uflags) =>
            {
                var flags = (NomenclatureSetFlag)uflags;
                string[] split = nomen.Split('@');
                if (split.Length != 2)
                {
                    return;
                }
                _mainWindowController.Locked = flags.HasFlag(NomenclatureSetFlag.Lock);
                _networkNameService.UpdateName(split[0], split[1]).ConfigureAwait(false);
            });
            
            _getNomenclature.RegisterFunc(() =>
            {
                if (IdentityService.CurrentNomenclature is null)
                    return "";
                return String.Concat([IdentityService.CurrentNomenclature.Name, "@", IdentityService.CurrentNomenclature.World]);

            });
        }
    }
}
