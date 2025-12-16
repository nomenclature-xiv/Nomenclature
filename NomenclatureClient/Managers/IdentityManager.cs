using System.Threading.Tasks;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.Base;
using NomenclatureCommon.Domain.Network.DeleteNomenclature;
using NomenclatureCommon.Domain.Network.UpdateNomenclature;

namespace NomenclatureClient.Managers;

public class IdentityManager(ConfigurationService configuration, NetworkService networkService)
{
    private const int MaxLength = 32;
    public async Task SetName(string name) => await Set(name, null, false);
    
    public async Task SetWorld(string world) => await Set(null, world, false);

    public async Task SetNameAndWorld(string? name, string? world, bool updateConfig = false) => await Set(name, world, updateConfig);
    
    public async Task ClearName() => await Clear(UpdateNomenclatureMode.Name);
    
    public async Task ClearWorld() => await Clear(UpdateNomenclatureMode.World);
    
    public async Task ClearNameAndWorld() => await Delete();

    public string GetDisplayName()
    {
        if (configuration.CharacterConfiguration is null)
            return string.Empty;

        var name = configuration.CharacterConfiguration.Name + '@' + configuration.CharacterConfiguration.World;
        if (IdentityService.Identities.TryGetValue(name, out var nomenclature))
        {
            var outname = nomenclature.Name ?? configuration.CharacterConfiguration.Name;
            var outworld = nomenclature.World ?? configuration.CharacterConfiguration.World;
            return string.Concat(outname, outworld == string.Empty ? "" : "@", outworld);
        }
        else
        {
            return name;
        }
    }

    public async Task Set(string? name, string? world, bool updateConfig)
    {
        if (name is null && world is null)
        {
            await Delete(updateConfig);
            return;
        }

        var mode = UpdateNomenclatureMode.None;
        if (name is not null)
            mode |= UpdateNomenclatureMode.Name;
        if (world is not null)
            mode |= UpdateNomenclatureMode.World;

        var request = new UpdateNomenclatureRequest(name, world, mode);
        var response = await networkService.InvokeAsync<Response>(HubMethod.UpdateNomenclature, request);
        if (response.Success is false)
            return;

        if (configuration.CharacterConfiguration is null)
            return;

        var player = configuration.CharacterConfiguration.Name + '@' + configuration.CharacterConfiguration.World;
        if (IdentityService.Identities.TryGetValue(player, out var nomenclature))
        {
            if (name is not null)
                nomenclature.Name = name;

            if (world is not null)
                nomenclature.World = world;
        }
        else
        {
            IdentityService.Identities.TryAdd(player, new Nomenclature(name, world));
        }
        if (updateConfig)
        {
            if (name is not null)
            {
                configuration.CharacterConfiguration.OverrideName = name;
            }

            if (world is not null)
            {
                configuration.CharacterConfiguration.OverrideWorld = world;
            }

            configuration.CharacterConfiguration.ShouldOverrideName = name is not null;
            configuration.CharacterConfiguration.ShouldOverrideWorld = world is not null;
            await configuration.SaveCharacterConfigurationAsync().ConfigureAwait(false);
        }
    }
    
    private async Task Clear(UpdateNomenclatureMode mode)
    {
        if (mode is UpdateNomenclatureMode.None)
            return;
        
        var request = new UpdateNomenclatureRequest(null, null, mode);
        var response = await networkService.InvokeAsync<Response>(HubMethod.UpdateNomenclature, request);
        if (response.Success is false)
            return;

        if (configuration.CharacterConfiguration is null)
            return;

        var player = configuration.CharacterConfiguration.Name + '@' + configuration.CharacterConfiguration.World;
        if (IdentityService.Identities.TryGetValue(player, out var nomenclature))
        {
            if ((mode & UpdateNomenclatureMode.Name) == UpdateNomenclatureMode.Name)
                nomenclature.Name = null;
            
            if ((mode & UpdateNomenclatureMode.World) == UpdateNomenclatureMode.World)
                nomenclature.World = null;
        }
        
        if ((mode & UpdateNomenclatureMode.Name) == UpdateNomenclatureMode.Name)
        {
            configuration.CharacterConfiguration.OverrideName = null;
            configuration.CharacterConfiguration.ShouldOverrideName = false;
        }
        
        if ((mode & UpdateNomenclatureMode.World) == UpdateNomenclatureMode.World)
        {
            configuration.CharacterConfiguration.OverrideWorld = null;
            configuration.CharacterConfiguration.ShouldOverrideWorld = false;
        }
    }

    private async Task Delete(bool updateConfig = false)
    {
        var request = new DeleteNomenclatureRequest();
        await networkService.InvokeAsync<Response>(HubMethod.DeleteNomenclature, request);
        if(updateConfig)
        {
            if (configuration.CharacterConfiguration is null)
                return;
            
            configuration.CharacterConfiguration.ShouldOverrideName = false;
            configuration.CharacterConfiguration.ShouldOverrideWorld = false;
        }
    }
}