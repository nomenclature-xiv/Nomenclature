using MessagePack;

namespace NomenclatureCommon.Domain.Network.Zones;

[MessagePackObject]
public record InsideZoneDto(
    [property: Key(0)] uint World,
    [property: Key(1)] uint Territory,
    [property: Key(2)] ulong House) : ZoneDto;