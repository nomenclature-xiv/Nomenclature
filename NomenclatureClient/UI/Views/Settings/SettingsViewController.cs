using System;
using NomenclatureClient.Managers;
using NomenclatureClient.Network;
using NomenclatureClient.Services;

namespace NomenclatureClient.UI.Views.Settings;

public class SettingsViewController
{
    private readonly ConfigurationService _configuration;
    private readonly NetworkService _networkService;
    
    public bool AutoLogin;

    public SettingsViewController(ConfigurationService configuration, NetworkService networkService, LoginManager loginManager)
    {
        _configuration = configuration;
        _networkService = networkService;
        
        loginManager.LoginFinished += OnLoginFinished;
    }
    public async void UpdateAutologin()
    {
        try
        {
            if (_configuration.CharacterConfiguration is null)
                return;

            _configuration.CharacterConfiguration.AutoConnect = this.AutoLogin;
            await _configuration.SaveCharacterConfigurationAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            //ignore
        }
    }

    public async void Disconnect()
    {
        try
        {
            await _networkService.Disconnect().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private void OnLoginFinished()
    {
        AutoLogin = _configuration.CharacterConfiguration?.AutoConnect ?? false;
    }
    
}