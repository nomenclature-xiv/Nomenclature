using MessagePack;

namespace NomenclatureCommon.Domain.Network.RemoveZone;

[MessagePackObject]
public record RemoveZoneResponse(
    [property: Key(0)] RequestErrorCode ErrorCode
    );