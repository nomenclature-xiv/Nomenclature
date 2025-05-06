using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using NomenclatureClient.Types;
using NomenclatureCommon.Domain;

namespace NomenclatureClient.Services;

public class CharacterService
{
    public Character? CurrentCharacter;
    public CharConfig? CurrentConfig;


    private readonly Configuration _configuration;
    private readonly IClientState _clientState;
    private readonly IFramework _framework;
    private readonly IPluginLog _pluginLog;

    public CharacterService(Configuration configuration, IClientState clientState, IFramework framework, IPluginLog pluginLog)
    {
        _configuration = configuration;
        _framework = framework;
        _clientState = clientState;
        _pluginLog = pluginLog;
        
    }
    
    public async Task<bool> OnLogin()
    {
        try
        {
            if (await RunOnFramework(() => _clientState.LocalPlayer).ConfigureAwait(false) is { } localPlayer)
            {
                var name = localPlayer.Name.ToString();
                var world = localPlayer.HomeWorld.Value.Name.ToString();
                CurrentCharacter = new Character(name, world);
                CurrentConfig = _configuration.LocalCharacters.GetValueOrDefault(string.Concat(name, "@", world));
                return true;
            }
            else
            {
                CurrentCharacter = null;
                CurrentConfig = null;
                return false;
            }
        }
        catch (Exception e)
        {
            _pluginLog.Info($"[CharacterService] [OnLogin] {e}");
            CurrentCharacter = null;
            CurrentConfig = null;
            return false;
        }
    }

    public void OnLogout(int type, int code)
    {
        CurrentCharacter = null;
        CurrentConfig = null;
    }

    private async Task<T> RunOnFramework<T>(Func<T> func)
    {
        if (_framework.IsInFrameworkUpdateThread)
            return func.Invoke();

        return await _framework.RunOnFrameworkThread(func).ConfigureAwait(false);
    }
}
