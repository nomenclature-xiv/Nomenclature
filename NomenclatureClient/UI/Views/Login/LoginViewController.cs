using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Managers;
using NomenclatureClient.Network;
using NomenclatureClient.Services;

// ReSharper disable RedundantBoolCompare

namespace NomenclatureClient.UI.Views.Login;

public class LoginViewController : IHostedService
{
    private readonly ConfigurationService _configuration;
    private readonly NetworkService _network;
    private readonly LoginManager _loginManager;
    private readonly IPluginLog _pluginLog;

    public LoginViewController(ConfigurationService configuration, NetworkService network, LoginManager loginManager, IPluginLog pluginLog)
    {
        _configuration = configuration;
        _network = network;
        _loginManager = loginManager;
        _pluginLog =  pluginLog;

        _loginManager.LoginFinished += OnLoginFinished;
    }
    
    public string SecretId = string.Empty;
    public bool AutoLogin;
    public bool Connected => _network.Connection.State is HubConnectionState.Connected;

    public async void Connect()
    {
        try
        {
            await _network.Connect().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Ignored
        }
    }
    
    public async void Disconnect()
    {
        try
        {
            await _network.Disconnect().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Ignored
        }
    }

    public async void UpdateSecretId(string secretId)
    {
        try
        {
            if (_configuration.CharacterConfiguration is null)
                return;
            
            SecretId = secretId;
            _configuration.CharacterConfiguration.SecretId = secretId;
            await _configuration.SaveCharacterConfigurationAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Ignore
        }
    }
    
    private void OnLoginFinished()
    {
        var secretId = _configuration.CharacterConfiguration?.SecretId ?? string.Empty;
        if (_configuration.Configuration.Secrets.ContainsKey(secretId) is false)
            secretId = string.Empty;
        
        SecretId = secretId;
        AutoLogin = _configuration.CharacterConfiguration?.AutoConnect ?? false;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _loginManager.LoginFinished -= OnLoginFinished;
        return Task.CompletedTask;
    }
}