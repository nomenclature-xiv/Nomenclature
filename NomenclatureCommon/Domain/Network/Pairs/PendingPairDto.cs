using MessagePack;

namespace NomenclatureCommon.Domain.Network.Pairs;

/// <summary>
///     A pair that is pending
/// </summary>
[MessagePackObject]
public sealed record PendingPairDto(
    string SyncCode
) : PairDto(SyncCode);