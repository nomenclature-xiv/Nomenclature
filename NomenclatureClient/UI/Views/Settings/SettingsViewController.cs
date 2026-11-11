using System;
using NomenclatureClient.Managers;
using NomenclatureClient.Network;
using NomenclatureClient.Services;

namespace NomenclatureClient.UI.Views.Settings;

public class SettingsViewController
{
    private readonly ConfigurationService _configuration;
    private readonly NetworkService _networkService;
    private readonly ZoneManager _zoneManager;
    
    public bool AutoLogin;
    public bool GlobalSync;

    public SettingsViewController(ConfigurationService configuration, NetworkService networkService, LoginManager loginManager, ZoneManager zoneManager)
    {
        _configuration = configuration;
        _networkService = networkService;
        _zoneManager = zoneManager;
        
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

    public async void UpdateGlobalSync()
    {
        try
        {
            if (_configuration.CharacterConfiguration is null)
                return;
            _configuration.CharacterConfiguration.GlobalSync = this.GlobalSync;
            await _configuration.SaveCharacterConfigurationAsync().ConfigureAwait(false);
            if (GlobalSync)
                await _zoneManager.UpdateCurrentZone();
            else
                await _zoneManager.RemoveCurrentZone();
        }
        catch (Exception ex)
        {
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