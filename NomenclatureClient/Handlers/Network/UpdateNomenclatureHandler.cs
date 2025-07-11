using Dalamud.Plugin.Services;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain.Network.UpdateNomenclature;

namespace NomenclatureClient.Handlers.Network;

public class UpdateNomenclatureHandler(IPluginLog logger, INamePlateGui namePlateGui)
{
    public void Handle(UpdateNomenclatureForwardedRequest request)
    {
        logger.Info($"{request}");
        IdentityService.Identities[request.CharacterName] = request.Nomenclature;
        namePlateGui.RequestRedraw();
    }
}