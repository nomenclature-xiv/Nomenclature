using Dalamud.Plugin.Services;
using Microsoft.AspNetCore.SignalR.Client;
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
        private readonly NameService _nameService;

        public LoginService(IClientState clientState, CharacterService characterService, Configuration configuration, NetworkHubService hubService, NameService nameService, IPluginLog pluginLog)
        {
            _clientState = clientState;
            _characterService = characterService;
            _config = configuration;
            _hubService = hubService;
            _nameService = nameService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _clientState.Login += OnLogin;
            _clientState.Logout += OnLogout;
            _hubService.Connected += OnConnected;
            OnLogin();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _clientState.Login -= OnLogin;
            _clientState.Logout -= OnLogout;
            return Task.CompletedTask;
        }

        private async Task OnConnected()
        {
            if (_config.AutoConnect && _characterService.CurrentConfig is not null)
            {
                await _nameService.ChangeName();
            }
        }

        private async void OnLogin()
        {
            await _characterService.OnLogin();
        }
        private async void OnLogout(int type, int code)
        {
            _characterService.OnLogout(type, code);
            await _hubService.Disconnect();
        }
    }
}
