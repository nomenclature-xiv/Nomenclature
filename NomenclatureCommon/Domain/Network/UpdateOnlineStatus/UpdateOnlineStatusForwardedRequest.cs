using MessagePack;
using NomenclatureCommon.Domain.Network.Pairs;

namespace NomenclatureCommon.Domain.Network.UpdateOnlineStatus;

[MessagePackObject]
public record UpdateOnlineStatusForwardedRequest(
    [property: Key(0)] PairDto Pair
);