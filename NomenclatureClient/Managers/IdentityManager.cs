using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using NomenclatureClient.Ipc;
using NomenclatureClient.Services;
using NomenclatureClient.Types;
using NomenclatureCommon.Domain;

namespace NomenclatureClient.Managers;

public class IdentityManager(
    SessionService sessionService,
    INamePlateGui namePlateGui)
{
    private const int MaxLength = 32;
    public void SetName(string? name) => Set(name, null, UpdateNomenclatureFlag.Name);
    
    public void SetWorld(string? world) => Set(null, world, UpdateNomenclatureFlag.World);

    public void SetNameAndWorld(string? name, string? world, bool updateConfig = false) => Set(name, world, UpdateNomenclatureFlag.Name | UpdateNomenclatureFlag.World, updateConfig);
    
    public async Task ClearName() => await Clear(UpdateNomenclatureFlag.Name);
    
    public async Task ClearWorld() => await Clear(UpdateNomenclatureFlag.World);
    
    public async Task ClearNameAndWorld() => await Delete();

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

    public Nomenclature? GetNomenclature()
    {
        var player = sessionService.CurrentSession.Character.ToString();
        IdentityService.Identities.TryGetValue(player, out var nomenclature);
        return nomenclature;
    }

    public void SetConfig(CharacterConfiguration configuration)
    {
        if(configuration.OverrideName)
        {
            Set(configuration.Name, null, UpdateNomenclatureFlag.Name);
        }
        if(configuration.OverrideWorld)
        {
            Set(null, configuration.World, UpdateNomenclatureFlag.World);
        }
    }

    private void Set(string? name, string? world, UpdateNomenclatureFlag flags, bool updateConfig = false)
    {
        if (flags is UpdateNomenclatureFlag.None)
            return;

        var player = sessionService.CurrentSession.Character.ToString();
        if (IdentityService.Identities.TryGetValue(player, out var nomenclature))
        {
            if (flags.HasFlag(UpdateNomenclatureFlag.Name))
                nomenclature.Name = name;

            if (flags.HasFlag(UpdateNomenclatureFlag.World))
                nomenclature.World = world;
        }
        else
        {
            nomenclature = new Nomenclature(name, world);
            IdentityService.Identities.TryAdd(player, nomenclature);
        }
        if (updateConfig)
        {
            if (name is not null)
            {
                sessionService.CurrentSession.CharacterConfiguration.Name = name;
            }
            if (world is not null)
            {
                sessionService.CurrentSession.CharacterConfiguration.World = world;
            }
            sessionService.CurrentSession.CharacterConfiguration.OverrideName = name != null;
            sessionService.CurrentSession.CharacterConfiguration.OverrideWorld = world != null;
            sessionService.Save();
        }
        IpcHandler.ChangedNomenclature(nomenclature);
        namePlateGui.RequestRedraw();
    }
    
    private async Task Clear(UpdateNomenclatureFlag mode)
    {
        if (mode is UpdateNomenclatureFlag.None)
            return;

        var player = sessionService.CurrentSession.Character.ToString();
        if (IdentityService.Identities.TryGetValue(player, out var nomenclature))
        {
            if (mode.HasFlag(UpdateNomenclatureFlag.Name))
            {
                nomenclature.Name = null;
            }
            if (mode.HasFlag(UpdateNomenclatureFlag.World))
            {
                nomenclature.World = null;
            }
        }
        IpcHandler.ChangedNomenclature(nomenclature);
        namePlateGui.RequestRedraw();
    }

    private async Task Delete()
    {
        var player = sessionService.CurrentSession.Character.ToString();
        IdentityService.Identities.TryRemove(player, out _);
        IpcHandler.ChangedNomenclature(null);
        namePlateGui.RequestRedraw();
    }
}