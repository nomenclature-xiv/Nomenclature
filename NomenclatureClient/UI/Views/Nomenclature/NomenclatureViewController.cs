using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NomenclatureClient.Managers;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureClient.Types.Extensions;
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
    private readonly IPluginLog _pluginLog;
    
    public readonly string[] Behaviors = Enum.GetValues<NomenclatureBehavior>().Select(x => x.GetDisplayName()).ToArray();

    public Types.Nomenclature? Nomenclature;
    
    public string NomenclatureName, NomenclatureWorld;
    public int NomenclatureNameBehaviorIndex, NomenclatureWorldBehaviorIndex;
    
    public NomenclatureViewController(ConfigurationService configuration, NetworkService networkService, LoginManager loginManager, NomenclatureService nomenclatureService, IPluginLog pluginLog)
    {
        _configuration = configuration;
        _networkService = networkService;
        _loginManager = loginManager;
        _loginManager.LoginFinished += OnLoginFinished;
        _nomenclatureService = nomenclatureService;
        _pluginLog = pluginLog;
        
        NomenclatureName = string.Empty;
        NomenclatureWorld = string.Empty;
        NomenclatureNameBehaviorIndex = 0;
        NomenclatureWorldBehaviorIndex = 0;
    }

    private void OnLoginFinished()
    {
        Nomenclature = _configuration.CharacterConfiguration?.Nomenclature ?? Types.Nomenclature.Empty;
        
        NomenclatureName = Nomenclature.Name;
        NomenclatureWorld = Nomenclature.World;
        NomenclatureNameBehaviorIndex = Math.Max((int)Nomenclature.NameBehavior, 0);
        NomenclatureWorldBehaviorIndex = Math.Max((int)Nomenclature.WorldBehavior, 0);
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
            return "<This space intentionally left blank>";

        return name + (nomenclature.WorldBehavior != NomenclatureBehavior.DisplayNothing ? " «" + world + "»" : "");
    }

    public string DisplayPendingNomenclature()
    {
        NomenclatureBehavior nameMode = (NomenclatureBehavior)NomenclatureNameBehaviorIndex;
        var name = nameMode switch
        {
            NomenclatureBehavior.DisplayOriginal => _configuration.CharacterConfiguration?.Name,
            NomenclatureBehavior.OverrideOriginal => NomenclatureName,
            NomenclatureBehavior.DisplayNothing => string.Empty,
            _ => "Unknown"
        };
        
        NomenclatureBehavior worldMode = (NomenclatureBehavior)NomenclatureWorldBehaviorIndex;
        var world = worldMode switch
        {
            NomenclatureBehavior.DisplayOriginal => _configuration.CharacterConfiguration?.World,
            NomenclatureBehavior.OverrideOriginal => NomenclatureWorld,
            NomenclatureBehavior.DisplayNothing => string.Empty,
            _ => "Unknown"
        };

        if (nameMode is NomenclatureBehavior.DisplayNothing && worldMode is NomenclatureBehavior.DisplayNothing)
            return "<This space intentionally left blank>";

        return name + (worldMode != NomenclatureBehavior.DisplayNothing ? " «" + world + "»" : "");
    }

    public async Task SubmitChanges()
    {
        // We shouldn't save changes if the character configuration isn't set
        if (_configuration.CharacterConfiguration is null)
            return;
        
        NomenclatureBehavior nameMode = (NomenclatureBehavior)NomenclatureNameBehaviorIndex;
        NomenclatureBehavior worldMode = (NomenclatureBehavior)NomenclatureWorldBehaviorIndex;
            
        // TODO: Validate NomenclatureName, NomenclatureWorld
        var nomenclature = new NomenclatureDto(NomenclatureName, nameMode, NomenclatureWorld, worldMode);
        var request = new UpdateNomenclatureRequest(nomenclature);
        var response = await _networkService.InvokeAsync<UpdateNomenclatureResponse>(HubMethod.UpdateNomenclature, request).ConfigureAwait(false);

        // TODO: Log failures
        if (response.ErrorCode is not RequestErrorCode.Success)
        {
            return;
        }

        // Convert back to domain
        var domain = nomenclature.ToNomenclature();
        
        // Set Nomenclature in service
        _nomenclatureService.Set(_configuration.CharacterConfiguration.Name, _configuration.CharacterConfiguration.World, domain);
        
        // Set Nomenclature for Ui editing purposes
        Nomenclature = domain;
        
        // Set and save Nomenclature
        _configuration.CharacterConfiguration.Nomenclature = domain;
        await _configuration.SaveCharacterConfigurationAsync().ConfigureAwait(false);
    }

    public async Task RemoveNomenclature()
    {
        // We shouldn't save changes if the character configuration isn't set
        if (_configuration.CharacterConfiguration is null)
            return;
        
        var request = new RemoveNomenclatureRequest();
        var response = await _networkService.InvokeAsync<RemoveNomenclatureResponse>(HubMethod.RemoveNomenclature, request).ConfigureAwait(false);
        if (response.ErrorCode is not RequestErrorCode.Success)
            return;
        
        // Remove our Nomenclature
        _nomenclatureService.RemoveNomenclatureForCharacter(_configuration.CharacterConfiguration.Name, _configuration.CharacterConfiguration.World);
        
        // Set and save an empty Nomenclature
        _configuration.CharacterConfiguration.Nomenclature = Types.Nomenclature.Empty;
        await _configuration.SaveCharacterConfigurationAsync().ConfigureAwait(false);
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