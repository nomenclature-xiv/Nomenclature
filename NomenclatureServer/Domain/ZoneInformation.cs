using NomenclatureCommon.Domain.Network.Zones;

namespace NomenclatureServer.Domain;

public record ZoneInformation(
    uint? Datacenter,
    uint? World,
    uint? Territory,
    sbyte? Ward,
    byte? Division,
    ulong? House)
{

    public static ZoneInformation? FromDto(ZoneDto dto) => dto switch
    {
        DutyZoneDto duty => new ZoneInformation(duty.Datacenter, null, duty.Territory, null, null, null),
        InsideZoneDto inside => new ZoneInformation(null, inside.World, inside.Territory, null, null, inside.House),
        OutsideZoneDto outside => new ZoneInformation(null, outside.World, outside.Territory, outside.Ward,
            outside.Division,
            null),
        _ => null
    };
}