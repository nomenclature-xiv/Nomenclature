using System;
using Dalamud.Plugin.Services;
using NomenclatureClient.Managers;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.Base;
using NomenclatureCommon.Domain.Network.DeleteNomenclature;
using NomenclatureCommon.Domain.Network.UpdateNomenclature;

namespace NomenclatureClient.UI;

public class MainWindowController(IPluginLog logger, IdentityManager identityManager, NetworkService networkService)
{
    public bool OverrideName = false;
    public string OverwrittenName = string.Empty;

    public bool OverrideWorld = false;
    public string OverwrittenWorld = string.Empty;
    
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

        await identityManager.SetNameAndWorld(name, world);
    }
    public async void ResetName()
    {
        await identityManager.ClearNameAndWorld();
    }
}