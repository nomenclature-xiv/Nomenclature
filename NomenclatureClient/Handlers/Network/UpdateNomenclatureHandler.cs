using Dalamud.Plugin.Services;
using NomenclatureCommon.Domain.Network.UpdateNomenclature;

namespace NomenclatureClient.Handlers.Network;

public class UpdateNomenclatureHandler(IPluginLog logger)
{
    public void Handle(UpdateNomenclatureForwardedRequest request)
    {
        logger.Verbose($"{request}");
        
        // TODO
    }
}