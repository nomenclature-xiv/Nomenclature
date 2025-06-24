using Dalamud.Plugin.Services;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain.Network.UpdateNomenclature;

namespace NomenclatureClient.Handlers.Network;

public class UpdateNomenclatureHandler(IPluginLog logger)
{
    public void Handle(UpdateNomenclatureForwardedRequest request)
    {
        logger.Info($"{request}");
        IdentityService.Identities.TryAdd(request.CharacterName, request.Nomenclature);
    }
}