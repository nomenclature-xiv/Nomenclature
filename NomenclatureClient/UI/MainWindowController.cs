using System;
using Dalamud.Plugin.Services;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain.Network;
using NomenclatureCommon.Domain.Network.Base;
using NomenclatureCommon.Domain.Network.DeleteNomenclature;
using NomenclatureCommon.Domain.Network.UpdateNomenclature;

namespace NomenclatureClient.UI;

public class MainWindowController(IPluginLog logger, NetworkService networkService)
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
        var mode = UpdateNomenclatureMode.None;
        if (OverrideName)
            mode |= UpdateNomenclatureMode.Name;
        if (OverrideWorld)
            mode |= UpdateNomenclatureMode.World;

        var name = OverrideName ? OverwrittenName : null;
        var world = OverrideWorld ? OverwrittenWorld : null;
        var update = new UpdateNomenclatureRequest(name, world, mode);

        var response = await networkService.InvokeAsync<Response>(HubMethod.UpdateNomenclature, update);
        if (response.Success is false)
        {
            logger.Warning("Unsuccessful!!!!");
        }
    }
    public async void ResetName()
    {
        var delete = new DeleteNomenclatureRequest();

        var response = await networkService.InvokeAsync<Response>(HubMethod.DeleteNomenclature, delete);
        if (response.Success is false)
        {
            logger.Warning("Unsuccessful!!!!");
        }
    }
}