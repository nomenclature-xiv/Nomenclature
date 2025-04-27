using NomenclatureClient.UI;
using NomenclatureCommon.Domain.Api.Base;
using NomenclatureCommon.Domain.Api.Server;
using NomenclatureCommon.Domain.Api;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NomenclatureCommon.Domain;
using Dalamud.Plugin.Services;

namespace NomenclatureClient.Network
{
    public class NetworkNameService
    {
        private readonly NetworkHubService _hubService;
        private readonly Configuration _config;
        private readonly IPluginLog _log;
        public NetworkNameService(NetworkHubService hubService, Configuration config, IPluginLog pluginLog)
        {
            _hubService = hubService;
            _config = config;
            _log = pluginLog;
        }
        public async void UpdateName(string? name, string? world)
        {
            try
            {
                var request = new SetNameRequest
                {
                    Nomenclature = new Nomenclature(name, world)
                };

                var response = await _hubService.InvokeAsync<SetNameRequest, Response>(ApiMethods.SetName, request);
                if (response.Success is false)
                    return;

                _config.Name = name;
                _config.World = world;
                _config.Save();
            }
            catch (Exception ex)
            {
                _log.Debug(ex.ToString());
            }
        }

        public async void ClearName()
        {
            try
            {
                var request = new ClearNameRequest();
                var response = await _hubService.InvokeAsync<ClearNameRequest, Response>(ApiMethods.ClearName, request);
                if (response.Success is false) _log.Debug("Could not clear name for some reason!");

            }
            catch (Exception ex)
            {
                _log.Debug(ex.ToString());
            }
        }
    }
}
