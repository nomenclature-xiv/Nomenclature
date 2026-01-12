using Dalamud.Plugin.Services;
using NomenclatureCommon.Domain.Network.UpdateOnlineStatus;

namespace NomenclatureClient.Handlers.Network;

public class UpdateOnlineStatusHandler(IPluginLog logger)
{
    public void Handle(UpdateOnlineStatusForwardedRequest request)
    {
        logger.Verbose($"{request}");
        
        // TODO
    }
}