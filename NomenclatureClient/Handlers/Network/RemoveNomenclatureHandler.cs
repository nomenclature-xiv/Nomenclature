using Dalamud.Plugin.Services;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain.Network.Pairs;
using NomenclatureCommon.Domain.Network.RemoveNomenclature;

namespace NomenclatureClient.Handlers.Network;

public class RemoveNomenclatureHandler(IPluginLog logger, NomenclatureService nomenclatures, PairService pairs)
{
    public void Handle(RemoveNomenclatureForwardedRequest request)
    {
        logger.Verbose($"{request}");

        if (pairs.TryGet(request.SyncCode) is not OnlinePairDto pair)
            return;
        
        nomenclatures.RemoveNomenclatureForCharacter(pair.CharacterName, pair.CharacterWorld);
    }
}