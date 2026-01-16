using MessagePack;

namespace NomenclatureCommon.Domain.Network.Pairs;

/// <summary>
///     A pair as represented
/// </summary>
/// <remarks>
///     See the valid types:<br/>
///     <see cref="PendingPairDto"/><br/>
///     <see cref="OfflinePairDto"/><br/>
///     <see cref="OnlinePairDto"/>
/// </remarks>
[MessagePackObject]
public abstract record PairDto(
    [property: Key(0)] string SyncCode
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