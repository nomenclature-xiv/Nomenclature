using Dalamud.Plugin.Services;
using NomenclatureCommon.Domain.Network.RemoveNomenclature;

namespace NomenclatureClient.Handlers.Network;

public class RemoveNomenclatureHandler(IPluginLog logger)
{
    public void Handle(RemoveNomenclatureForwardedRequest request)
    {
        logger.Verbose($"{request}");
        
        // TODO:
    }
}