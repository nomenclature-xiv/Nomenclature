using MessagePack;

namespace NomenclatureCommon.Domain.Network.Pairs.RemovePair;

[MessagePackObject]
public record RemovePairRequest(
    [property: Key(0)] string TargetPairCode
);