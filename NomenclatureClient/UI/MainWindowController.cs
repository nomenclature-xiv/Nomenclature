using System;
using Dalamud.Plugin.Services;
using NomenclatureClient.Managers;

namespace NomenclatureClient.UI;

public class MainWindowController(IPluginLog logger, IdentityManager identityManager)
{
    public bool OverrideName = false;
    public string OverwrittenName = string.Empty;

    public bool OverrideWorld = false;
    public string OverwrittenWorld = string.Empty;

    public void ChangeName()
    {
        var name = OverrideName ? OverwrittenName : null;
        var world = OverrideWorld ? OverwrittenWorld : null;

        identityManager.SetNameAndWorld(name, world, true);
    }
    public async void ResetName()
    {
        await identityManager.ClearNameAndWorld();
    }
}