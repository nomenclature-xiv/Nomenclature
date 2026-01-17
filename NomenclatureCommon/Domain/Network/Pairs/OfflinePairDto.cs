using MessagePack;

namespace NomenclatureCommon.Domain.Network.Pairs;

/// <summary>
///     A two-way pair that is offline
/// </summary>
[MessagePackObject]
public sealed record OfflinePairDto(
    string SyncCode
) : PairDto(SyncCode);