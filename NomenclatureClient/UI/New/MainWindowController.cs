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


    public async void ChangeName(string? name, string? world)
    {
        await _networkNameService.ClearName();
        if (name is null && world is null)
        {
            return;
        }
        var result = await _networkNameService.UpdateName(name, world);
    }
}