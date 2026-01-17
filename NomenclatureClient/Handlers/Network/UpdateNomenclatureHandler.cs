using Dalamud.Plugin.Services;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain.Network.Pairs;
using NomenclatureCommon.Domain.Network.UpdateNomenclature;

namespace NomenclatureClient.Handlers.Network;

public class UpdateNomenclatureHandler(IPluginLog logger, NomenclatureService nomenclatures, PairService pairs)
{
    public void Handle(UpdateNomenclatureForwardedRequest request)
    {
        logger.Verbose($"{request}");
     
        if (pairs.TryGet(request.SyncCode) is not OnlinePairDto pair)
            return;
        
        nomenclatures.Set(pair.CharacterName, pair.CharacterWorld, pair.Nomenclature);
    }
}