using MessagePack;

namespace NomenclatureCommon.Domain.Network.Zones;

[MessagePackObject]
public record OutsideZoneDto(
    [property: Key(0)] uint World,
    [property: Key(1)] uint Territory,
    [property: Key(2)] sbyte? Ward,
    [property: Key(3)] byte? Division) : ZoneDto;