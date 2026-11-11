using NomenclatureClient.Managers;
using NomenclatureClient.Services;
using NomenclatureClient.Types.Extensions;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Network.UpdateZone;

namespace NomenclatureClient.Handlers.Network;

public class UpdateNomenclatureZoneHandler(ConfigurationService configurationService, NomenclatureService nomenclatureService, ZoneManager zoneManager)
{
    public void Handle(UpdateNomenclatureZoneForwardedRequest forwardedRequest)
    {
        if ((configurationService.CharacterConfiguration?.GlobalSync ?? false) && forwardedRequest.NomenclatureDto is not null)
        {
            zoneManager.AddZonemenclature(new Character(forwardedRequest.CharacterName, forwardedRequest.CharacterWorld), forwardedRequest.NomenclatureDto.ToNomenclature());
        }
    }
}