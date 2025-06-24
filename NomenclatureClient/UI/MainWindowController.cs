using System;
using NomenclatureClient.Network;
using NomenclatureClient.Services;

namespace NomenclatureClient.UI;

public class MainWindowController(NetworkService networkService)
{
    public bool OverrideName = false;
    public string OverwrittenName = string.Empty;

    public bool OverrideWorld = false;
    public string OverwrittenWorld = string.Empty;
    
    public async void Disconnect()
    {
        try
        {
            await networkService.Disconnect();
        }
        catch (Exception)
        {
            // Ignore
        }
    }
}