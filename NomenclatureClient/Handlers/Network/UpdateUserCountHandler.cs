using Dalamud.Plugin.Services;
using NomenclatureClient.Network;
using NomenclatureCommon.Domain.Network.UpdateUserCount;

namespace NomenclatureClient.Handlers.Network;

public class UpdateUserCountHandler(IPluginLog logger, NetworkService networkService)
{
    public void Handle(UpdateUserCountForwardedRequest request)
    {
        logger.Verbose($"{request}");
        networkService.UserCount = request.UserCount;
    }
}