using MessagePack;

namespace NomenclatureCommon.Domain.Network.Pairs.PausePair;

[MessagePackObject]
public record PausePairRequest(
    [property: Key(0)] string TargetPairCode,
    [property: Key(1)] bool Pause
);