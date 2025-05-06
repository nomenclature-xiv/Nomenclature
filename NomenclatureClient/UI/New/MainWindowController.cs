using NomenclatureClient.Network;
using NomenclatureClient.Services;

namespace NomenclatureClient.UI.New;

public class MainWindowController
{
    private readonly NetworkNameService _networkNameService;
    private readonly Configuration _configuration;

    public MainWindowController(NetworkNameService networkNameService, Configuration config)
    {
        _networkNameService = networkNameService;
        _configuration = config;
    }


    public async void ChangeName(string? name, string? world)
    {
        await _networkNameService.ClearName();
        if (name is null && world is null)
        {
            return;
        }
        if(await _networkNameService.UpdateName(name, world))
        {
            _configuration.Save();
        }
    }
}