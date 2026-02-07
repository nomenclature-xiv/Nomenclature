using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Managers;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.RemoveNomenclature;
using NomenclatureCommon.Domain.Network.UpdateNomenclature;

namespace NomenclatureClient.UI.Views.Nomenclature;

// TODO: Handle Behaviors and conversions between String <-> Enum better
public class NomenclatureViewController : IHostedService
{
    private readonly ConfigurationService _configuration;
    private readonly NetworkService _networkService;
    private readonly LoginManager _loginManager;
    private readonly NomenclatureService _nomenclatureService;
    
    public readonly string[] Behaviors = Enum.GetNames<NomenclatureBehavior>();

    public NomenclatureCommon.Domain.Nomenclature? Nomenclature;
    
    public string NomenclatureName, NomenclatureWorld;
    public int NomenclatureNameBehaviorIndex, NomenclatureWorldBehaviorIndex;
    
    public NomenclatureViewController(ConfigurationService configuration, NetworkService networkService, LoginManager loginManager, NomenclatureService nomenclatureService)
    {
        _configuration = configuration;
        _networkService = networkService;
        _loginManager = loginManager;
        _loginManager.LoginFinished += OnLoginFinished;
        _nomenclatureService = nomenclatureService;
        
        NomenclatureName = string.Empty;
        NomenclatureWorld = string.Empty;
        NomenclatureNameBehaviorIndex = 0;
        NomenclatureWorldBehaviorIndex = 0;
    }

    private void OnLoginFinished()
    {
        Nomenclature = _configuration.CharacterConfiguration?.Nomenclature ?? new NomenclatureCommon.Domain.Nomenclature();
        
        NomenclatureName = Nomenclature.Name;
        NomenclatureWorld = Nomenclature.World;
        NomenclatureNameBehaviorIndex = Math.Max(Behaviors.IndexOf(Enum.GetName(Nomenclature.NameBehavior)), 0);
        NomenclatureWorldBehaviorIndex = Math.Max(Behaviors.IndexOf(Enum.GetName(Nomenclature.WorldBehavior)), 0);
    }

    public string DisplayCurrentNomenclature()
    {
        if (_configuration.CharacterConfiguration is null)
            return "Unknown";

        var nomenclature = _configuration.CharacterConfiguration.Nomenclature;
        
        var name = nomenclature.NameBehavior switch
        {
            NomenclatureBehavior.DisplayOriginal => _configuration.CharacterConfiguration.Name,
            NomenclatureBehavior.OverrideOriginal => nomenclature.Name,
            NomenclatureBehavior.DisplayNothing => string.Empty,
            _ => "Unknown"
        };
        
        var world = nomenclature.WorldBehavior switch
        {
            NomenclatureBehavior.DisplayOriginal => _configuration.CharacterConfiguration.World,
            NomenclatureBehavior.OverrideOriginal => nomenclature.World,
            NomenclatureBehavior.DisplayNothing => string.Empty,
            _ => "Unknown"
        };

        if (nomenclature.NameBehavior is NomenclatureBehavior.DisplayNothing && nomenclature.WorldBehavior is NomenclatureBehavior.DisplayNothing)
            return "<You are not displaying a name or world>";

        return name + " «" + world + "»";
    }

    public string DisplayPendingNomenclature()
    {
        Enum.TryParse<NomenclatureBehavior>(Behaviors[NomenclatureNameBehaviorIndex], out var nameMode);
        var name = nameMode switch
        {
            NomenclatureBehavior.DisplayOriginal => _configuration.CharacterConfiguration?.Name,
            NomenclatureBehavior.OverrideOriginal => NomenclatureName,
            NomenclatureBehavior.DisplayNothing => string.Empty,
            _ => "Unknown"
        };
        
        Enum.TryParse<NomenclatureBehavior>(Behaviors[NomenclatureWorldBehaviorIndex], out var worldMode);
        var world = worldMode switch
        {
            NomenclatureBehavior.DisplayOriginal => _configuration.CharacterConfiguration?.World,
            NomenclatureBehavior.OverrideOriginal => NomenclatureWorld,
            NomenclatureBehavior.DisplayNothing => string.Empty,
            _ => "Unknown"
        };

        if (nameMode is NomenclatureBehavior.DisplayNothing && worldMode is NomenclatureBehavior.DisplayNothing)
            return "<You are not displaying a name or world>";

        return name + " «" + world + "»";
    }

    public async void SubmitChanges()
    {
        try
        {
            var nameBehavior = Enum.TryParse<NomenclatureBehavior>(Behaviors[NomenclatureNameBehaviorIndex], out var nameMode) ? nameMode : NomenclatureBehavior.DisplayOriginal;
            var worldBehavior = Enum.TryParse<NomenclatureBehavior>(Behaviors[NomenclatureWorldBehaviorIndex], out var worldMode) ? worldMode : NomenclatureBehavior.DisplayOriginal;
            
            var nomenclature = new NomenclatureCommon.Domain.Nomenclature(NomenclatureName, nameBehavior, NomenclatureWorld, worldBehavior);
            var request = new UpdateNomenclatureRequest(nomenclature);
            
            var response = await _networkService.InvokeAsync<UpdateNomenclatureResponse>(HubMethod.UpdateNomenclature, request).ConfigureAwait(false);

            if (response.ErrorCode is not RequestErrorCode.Success)
                return;

            _nomenclatureService.Set(_configuration.CharacterConfiguration.Name, _configuration.CharacterConfiguration.World, nomenclature);
            Nomenclature = nomenclature;
            _configuration.CharacterConfiguration?.Nomenclature = nomenclature;
            await _configuration.SaveCharacterConfigurationAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Ignored
        }
    }

    public async void RemoveNomenclature()
    {
        try
        {
            var request = new RemoveNomenclatureRequest();
            var response = await _networkService.InvokeAsync<RemoveNomenclatureResponse>(HubMethod.RemoveNomenclature, request).ConfigureAwait(false);
            if (response.ErrorCode is not RequestErrorCode.Success)
                return;
            _nomenclatureService.RemoveNomenclatureForCharacter(_configuration.CharacterConfiguration.Name, _configuration.CharacterConfiguration.World);
            // Do something
        }
        catch (Exception)
        {
            // Ignore
        }
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