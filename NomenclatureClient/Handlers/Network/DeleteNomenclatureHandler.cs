using Dalamud.Plugin.Services;
using NomenclatureClient.Services;
using NomenclatureCommon.Domain.Network.DeleteNomenclature;

namespace NomenclatureClient.Handlers.Network;

public class DeleteNomenclatureHandler(IPluginLog logger, INamePlateGui namePlateGui)
{
    public void Handle(DeleteNomenclatureForwardedRequest request)
    {
        logger.Info($"{request}");
        IdentityService.Identities.TryRemove(request.CharacterName, out _);
        namePlateGui.RequestRedraw();
    }
}