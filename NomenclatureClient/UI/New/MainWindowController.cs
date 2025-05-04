using NomenclatureClient.Network;
using NomenclatureClient.Services;

namespace NomenclatureClient.UI.New;

public class MainWindowController
{
    public string NewName = string.Empty;
    public string NewWorld = string.Empty;
    public bool ShouldChangeName;
    public bool ShouldChangeWorld;
    
    private readonly NetworkNameService _networkNameService;

    public MainWindowController(NetworkNameService networkNameService)
    {
        _networkNameService = networkNameService;
    }


    public async void ChangeName(string name, string world)
    {
        var result = await _networkNameService.UpdateName(name, world);
    }
}