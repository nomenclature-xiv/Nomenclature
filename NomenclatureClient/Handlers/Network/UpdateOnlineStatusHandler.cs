using Dalamud.Plugin.Services;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain.Network.UpdateOnlineStatus;

namespace NomenclatureClient.Handlers.Network;

public class UpdateOnlineStatusHandler(IPluginLog logger, PairService pairs)
{
    public void Handle(UpdateOnlineStatusForwardedRequest request)
    {
        logger.Verbose($"{request}");

        if (pairs.TryGet(request.Pair.SyncCode) is null)
            return;
        
        pairs.Add(request.Pair);
    }
}