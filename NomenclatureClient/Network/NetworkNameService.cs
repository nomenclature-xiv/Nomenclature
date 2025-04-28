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
        public async Task<bool> UpdateName(string? name, string? world)
        {
            try
            {
                var request = new PublishNomenclatureRequest
                {
                    Nomenclature = new Nomenclature(name, world)
                };

                var response = await _hubService.InvokeAsync<PublishNomenclatureRequest, Response>(ApiMethods.PublishNomenclature, request);
                return response.Success;
            }
            catch (Exception ex)
            {
                _log.Debug(ex.ToString());
                return false;
            }
        }

        public async Task ClearName()
        {
            try
            {
                var request = new ResetNomenclatureRequest();
                var response = await _hubService.InvokeAsync<ResetNomenclatureRequest, Response>(ApiMethods.ResetNomenclature, request);
                if (response.Success is false) _log.Debug("Could not clear name for some reason!");

            }
            catch (Exception ex)
            {
                _log.Debug(ex.ToString());
            }
        }
    }
}
