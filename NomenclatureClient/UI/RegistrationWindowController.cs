using Dalamud.Plugin.Services;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NomenclatureClient.UI
{
    public class RegistrationWindowController
    {
        private readonly CharacterService _characterService;
        private readonly NetworkRegisterService _networkRegisterService;
        private readonly NetworkHubService _networkHubService;
        private readonly Configuration _configuration;
        private readonly IPluginLog _pluginLog;

        public string RegistrationKey = string.Empty;
        public bool CharacterError = false;
        public bool RegistrationError = false;
        

        public RegistrationWindowController(CharacterService characterService, NetworkRegisterService networkRegisterService, NetworkHubService networkHubService, Configuration configuration, IPluginLog pluginLog)
        {
            _characterService = characterService;
            _networkRegisterService = networkRegisterService;
            _networkHubService = networkHubService;
            _configuration = configuration;
            _pluginLog = pluginLog;
        }
        public async void InitRegister()
        {
            try
            {
                var name = _characterService.CurrentCharacter;
                if (name is null) return;
                var result = await _networkRegisterService.RegisterCharacterInitiate(name);
                _pluginLog.Info($"Result: {result}");
                if (result is not null)
                {
                    RegistrationKey = result;
                    CharacterError = false;
                }
                else
                {
                    CharacterError = true;
                }
            }
            catch (Exception e)
            {
                _pluginLog.Error($"{e}");
            }
        }

        public async Task<bool> ValidateRegister()
        {
            try
            {
                var name = _characterService.CurrentCharacter;
                if (name is null) return false;
                var result = await _networkRegisterService.RegisterCharacterValidate(name, RegistrationKey);
                if (result is not null)
                {
                    RegistrationError = false;
                    _configuration.LocalCharacters.TryGetValue(name.Name, out Dictionary<string, string>? worldsecret);
                    if (worldsecret is null)
                    {
                        worldsecret = new Dictionary<string, string>();
                    }
                    worldsecret.Add(name.World, result);
                    _configuration.LocalCharacters[name.Name] = worldsecret;
                    _configuration.Save();
                    await _networkHubService.Connect().ConfigureAwait(false);
                }
                else
                {
                    RegistrationError = true;
                }
                return RegistrationError;
            }
            catch (Exception e)
            {
                _pluginLog.Error($"{e}");
                return false;
            }
        }
    }
}
