using MessagePack;
using NomenclatureCommon.Domain.Network.Zones;

namespace NomenclatureCommon.Domain.Network.UpdateZone;

[MessagePackObject]
public record UpdateZoneResponse(
    [property: Key(0)] List<ZonemenclatureDto> Zonemenclatures);