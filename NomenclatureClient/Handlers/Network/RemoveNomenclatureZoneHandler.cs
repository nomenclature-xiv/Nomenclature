using NomenclatureClient.Services;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Network.RemoveZone;
using NomenclatureClient.Managers;

namespace NomenclatureClient.Handlers.Network;
public class RemoveNomenclatureZoneHandler(ConfigurationService configurationService, NomenclatureService nomenclatureService, ZoneManager zoneManager)
{
    public void Handle(RemoveNomenclatureZoneForwardedRequest request)
    {
        if ((configurationService.CharacterConfiguration?.GlobalSync ?? false))
        {
            zoneManager.RemoveZonemenclature(new Character(request.CharacterName, request.CharacterWorld));
        }
    }
}