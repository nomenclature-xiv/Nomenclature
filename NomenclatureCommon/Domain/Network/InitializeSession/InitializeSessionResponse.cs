using MessagePack;
using NomenclatureCommon.Domain.Network.Pairs;
using NomenclatureCommon.Domain.Network.Zones;

namespace NomenclatureCommon.Domain.Network.InitializeSession;

[MessagePackObject]
public record InitializeSessionResponse(
    [property: Key(0)] RequestErrorCode ErrorCode,
    [property: Key(1)] string SyncCode,
    [property: Key(2)] List<PairDto> Pairs,
    [property: Key(3)] List<ZonemenclatureDto>? ZoneNomenclatureDto
);