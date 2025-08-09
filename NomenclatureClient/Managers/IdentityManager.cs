using System.Threading.Tasks;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.Base;
using NomenclatureCommon.Domain.Network.UpdateNomenclature;

namespace NomenclatureClient.Managers;

public class IdentityManager(
    NetworkService networkService,
    SessionService sessionService)
{
    private const int MaxLength = 32;
    public async Task SetName(string name) => await Set(name, null);
    
    public async Task SetWorld(string world) => await Set(null, world);
    
    public async Task ClearName() => await Clear(UpdateNomenclatureMode.Name);
    
    public async Task ClearWorld() => await Clear(UpdateNomenclatureMode.World);
    
    public async Task ClearNameAndWorld() => await Clear(UpdateNomenclatureMode.Name | UpdateNomenclatureMode.World);

    public string GetDisplayName()
    {
        var player = sessionService.CurrentSession.Character;
        var playername = player.ToString();
        if (IdentityService.Identities.TryGetValue(playername, out var nomenclature))
        {
            var outname = nomenclature.Name ?? player.Name;
            var outworld = nomenclature.World ?? player.World;
            return string.Concat(outname, outworld == string.Empty ? "" : "@", outworld);
        }
        else
        {
            return playername;
        }
    }

    private async Task Set(string? name, string? world)
    {
        if (name is null && world is null)
            return;

        var mode = UpdateNomenclatureMode.None;
        if (name is not null)
            mode |= UpdateNomenclatureMode.Name;
        if (world is not null)
            mode |= UpdateNomenclatureMode.World;

        var request = new UpdateNomenclatureRequest(name, world, mode);
        var response = await networkService.InvokeAsync<Response>(HubMethod.UpdateNomenclature, request);
        if (response.Success is false)
            return;

        var player = sessionService.CurrentSession.Character.ToString();
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

        if (name is not null)
        {
            sessionService.CurrentSession.CharacterConfiguration.Name = name;
            sessionService.CurrentSession.CharacterConfiguration.OverrideName = true;
        }

        if (world is not null)
        {
            sessionService.CurrentSession.CharacterConfiguration.World = world;
            sessionService.CurrentSession.CharacterConfiguration.OverrideWorld = true;
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

        var player = sessionService.CurrentSession.Character.ToString();
        if (IdentityService.Identities.TryGetValue(player, out var nomenclature))
        {
            if ((mode & UpdateNomenclatureMode.Name) == UpdateNomenclatureMode.Name)
                nomenclature.Name = null;
            
            if ((mode & UpdateNomenclatureMode.World) == UpdateNomenclatureMode.World)
                nomenclature.World = null;
        }
        
        if ((mode & UpdateNomenclatureMode.Name) == UpdateNomenclatureMode.Name)
        {
            sessionService.CurrentSession.CharacterConfiguration.Name = null;
            sessionService.CurrentSession.CharacterConfiguration.OverrideName = false;
        }
        
        if ((mode & UpdateNomenclatureMode.World) == UpdateNomenclatureMode.World)
        {
            sessionService.CurrentSession.CharacterConfiguration.World = null;
            sessionService.CurrentSession.CharacterConfiguration.OverrideWorld = false;
        }
    }
}