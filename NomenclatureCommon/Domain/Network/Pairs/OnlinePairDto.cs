using MessagePack;

namespace NomenclatureCommon.Domain.Network.Pairs;

/// <summary>
///     A two-way pair that is online
/// </summary>
[MessagePackObject]
public sealed record OnlinePairDto(
    string SyncCode,
    [property: Key(2)] bool LeftSidePaused,
    [property: Key(3)] bool RightSidePaused,
    [property: Key(4)] NomenclatureDto? NomenclatureDto,
    [property: Key(5)] string CharacterName,
    [property: Key(6)] string CharacterWorld
) : PairDto(SyncCode);