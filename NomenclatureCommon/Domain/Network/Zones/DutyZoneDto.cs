using MessagePack;

namespace NomenclatureCommon.Domain.Network.Zones;

[MessagePackObject]
public record DutyZoneDto(
    [property: Key(0)] uint Datacenter,
    [property: Key(1)] uint Territory) : ZoneDto;