using MessagePack;
using NomenclatureCommon.Domain.Network.Pairs;

namespace NomenclatureCommon.Domain.Network.InitializeSession;

[MessagePackObject]
public record InitializeSessionResponse(
    [property: Key(0)] RequestErrorCode ErrorCode,
    [property: Key(1)] string SyncCode,
    [property: Key(2)] List<PairDto> Pairs
);