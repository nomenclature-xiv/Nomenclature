using MessagePack;

namespace NomenclatureCommon.Domain.Network.Pairs.AddPair;

[MessagePackObject]
public record AddPairRequest(
    [property: Key(0)] string TargetPairCode
);