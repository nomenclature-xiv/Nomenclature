using MessagePack;
using NomenclatureCommon.Domain.Network.Zones;

namespace NomenclatureCommon.Domain.Network.UpdateZone;

[MessagePackObject]
public record UpdateZoneRequest(
    [property: Key(0)] ZoneDto Zone);