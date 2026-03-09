using MessagePack;

namespace NomenclatureCommon.Domain.Network.Pairs;

/// <summary>
///     A two-way pair that is online
/// </summary>
[MessagePackObject]
public sealed record OnlinePairDto(
    string SyncCode,
    [property: Key(2)] NomenclatureDto? NomenclatureDto,
    [property: Key(3)] string CharacterName,
    [property: Key(4)] string CharacterWorld
) : PairDto(SyncCode);