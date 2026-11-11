using Dalamud.Plugin.Services;
using NomenclatureClient.Services;
using NomenclatureClient.Types.Extensions;
using NomenclatureCommon.Domain.Network.Pairs;
using NomenclatureCommon.Domain.Network.UpdateOnlineStatus;

namespace NomenclatureClient.Handlers.Network;

public class UpdateOnlineStatusHandler(IPluginLog logger, PairService pairs, NomenclatureService nomenclatureService)
{
    public void Handle(UpdateOnlineStatusForwardedRequest request)
    {
        logger.Verbose($"{request}");

        if (pairs.TryGet(request.Pair.SyncCode) is null)
            return;
        
        pairs.Add(request.Pair);
        if(request.Pair is OnlinePairDto pair && pair.NomenclatureDto is not null)
            nomenclatureService.Set(pair.CharacterName, pair.CharacterWorld, pair.NomenclatureDto.ToNomenclature());
    }
}