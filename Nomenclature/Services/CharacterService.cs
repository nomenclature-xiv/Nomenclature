using System;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using NomenclatureCommon.Domain;

namespace Nomenclature.Services;

public class CharacterService : IDisposable
{
    public Character? CurrentCharacter;

    private readonly IClientState _clientState;
    private readonly IFramework _framework;
    private readonly IPluginLog _pluginLog;

    public CharacterService(IClientState clientState, IFramework framework, IPluginLog pluginLog)
    {
        _framework = framework;
        _clientState = clientState;
        _pluginLog = pluginLog;
        
        _clientState.Login += OnLogin;
        _clientState.Logout += OnLogout;
        OnLogin();
    }

    public void Dispose()
    {
        _clientState.Login -= OnLogin;
        _clientState.Logout -= OnLogout;
        GC.SuppressFinalize(this);
    }
    
    private async void OnLogin()
    {
        try
        {
            if (await RunOnFramework(() => _clientState.LocalPlayer).ConfigureAwait(false) is { } localPlayer)
            {
                CurrentCharacter = new Character(localPlayer.Name.ToString(), localPlayer.HomeWorld.Value.Name.ToString());
            }
            else
            {
                CurrentCharacter = null;
            }
        }
        catch (Exception e)
        {
            _pluginLog.Info($"[CharacterService] [OnLogin] {e}");
        }
    }

    private void OnLogout(int type, int code)
    {
        CurrentCharacter = null;
    }

    private async Task<T> RunOnFramework<T>(Func<T> func)
    {
        if (_framework.IsInFrameworkUpdateThread)
            return func.Invoke();

        return await _framework.RunOnFrameworkThread(func).ConfigureAwait(false);
    }
}
