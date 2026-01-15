using MessagePack;

namespace NomenclatureCommon.Domain.Network;

[MessagePackObject]
public record PairDto(
    [property: Key(0)] string SyncCode,
    [property: Key(1)] OnlineStatus Status,
    [property: Key(2)] bool LeftSidePaused,
    [property: Key(3)] bool? RightSidePaused,
    [property: Key(4)] Nomenclature? Nomenclature
);

/*
{
[Key(0)] public string SyncCode { set; get; } = string.Empty;
[Key(1)] public bool LeftSidePaused { set; get; }
[Key(2)] public bool? RightSidePaused { set; get; }
[Key(3)] public Nomenclature? Nomenclature { set; get; }

public PairDto()
{
}

public PairDto(string syncCode, bool leftSidePaused, bool? rightSidePaused, Nomenclature? nomenclature)
{
    SyncCode = syncCode;
    LeftSidePaused = leftSidePaused;
    RightSidePaused = rightSidePaused;
    Nomenclature = nomenclature;
}
*/