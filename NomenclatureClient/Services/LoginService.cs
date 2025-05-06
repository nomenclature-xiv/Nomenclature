using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.BannerHelper.Delegates;

namespace NomenclatureClient.Services
{
    public class LoginService : IHostedService
    {
        private readonly IClientState _clientState;
        private readonly CharacterService _characterService;
        private readonly Configuration _config;
        private readonly NetworkHubService _hubService;

        public LoginService(IClientState clientState, CharacterService characterService, Configuration configuration, NetworkHubService hubService, IPluginLog pluginLog)
        {
            _clientState = clientState;
            _characterService = characterService;
            _config = configuration;
            _hubService = hubService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _clientState.Login += OnLogin;
            _clientState.Logout += OnLogout;
            OnLogin();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _clientState.Login -= OnLogin;
            _clientState.Logout -= OnLogout;
            return Task.CompletedTask;
        }

        private async void OnLogin()
        {
            if(await _characterService.OnLogin() && _config.AutoConnect && _characterService.CurrentConfig.Secret is not null)
            {
                await _hubService.Connect();
            }
        }
        private async void OnLogout(int type, int code)
        {
            _characterService.OnLogout(type, code);
            await _hubService.Disconnect();
        }
    }
}
