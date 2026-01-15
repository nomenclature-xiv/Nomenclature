using MessagePack;

namespace NomenclatureCommon.Domain.Network.UpdateOnlineStatus;

[MessagePackObject]
public record UpdateOnlineStatusForwardedRequest(
    [property: Key(0)] string SyncCode,
    [property: Key(1)] OnlineStatus Status,
    [property: Key(2)] Nomenclature? Nomenclature
);
/*
{
    [Key(0)] public string SyncCode { get; set; } = string.Empty;
    [Key(1)] public OnlineStatus Status { get; set; }
    [Key(2)] public PairInformation? Information { get; set; }
    [Key(3)] public Nomenclature? Nomenclature { get; set; }

    public UpdateOnlineStatusForwardedRequest()
    {
    }

    public UpdateOnlineStatusForwardedRequest(string syncCode, OnlineStatus status, PairInformation? information, Nomenclature? nomenclature)
    {
        SyncCode = syncCode;
        Status = status;
        Information = information;
        Nomenclature = nomenclature;
    }
}
*/