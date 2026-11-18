using MessagePack;
using NomenclatureCommon.Domain.Network.Zones;

namespace NomenclatureCommon.Domain.Network.UpdateZone;

[MessagePackObject]
public record UpdateZoneResponse
{
    [Key(0)] public List<ZonemenclatureDto>? Zonemenclatures { get; set; }
    public UpdateZoneResponse()
    {
        
    }
    
    public UpdateZoneResponse(List<ZonemenclatureDto>? zonemenclatures)
    {
        Zonemenclatures = zonemenclatures;
    }
}