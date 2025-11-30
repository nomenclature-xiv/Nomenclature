using System;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using NomenclatureClient.Managers;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.Base;
using NomenclatureCommon.Domain.Network.DeleteNomenclature;
using NomenclatureCommon.Domain.Network.UpdateNomenclature;

namespace NomenclatureClient.UI;

public class MainWindowController(IPluginLog logger, IFramework framework, IClientState clientState, IdentityManager identityManager, NetworkService networkService, NetworkRegisterService registerService)
{
    public bool OverrideName = false;
    public string OverwrittenName = string.Empty;

    public bool OverrideWorld = false;
    public string OverwrittenWorld = string.Empty;

    public async void StartRegistration()
    {
        if (await framework.RunOnFrameworkThread(() => clientState.LocalPlayer) is not { } player)
            return;
        await registerService.RegisterCharacter(new Character(player.Name.TextValue, player.HomeWorld.Value.Name.ToString()));
    }
    
    public async void TryConnect()
    {
        try
        {
            await networkService.Connect();
        }
        catch (Exception)
        {
            // Ignore
        }
    }

    public async void ChangeName()
    {
        var name = OverrideName ? OverwrittenName : null;
        var world = OverrideWorld ? OverwrittenWorld : null;

        await identityManager.Set(name, world, true);
    }
    public async void ResetName()
    {
        await identityManager.ClearNameAndWorld();
    }
}