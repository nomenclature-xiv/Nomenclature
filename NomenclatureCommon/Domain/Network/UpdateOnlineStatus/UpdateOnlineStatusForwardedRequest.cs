using MessagePack;

namespace NomenclatureCommon.Domain.Network.UpdateOnlineStatus;

[MessagePackObject]
public record UpdateOnlineStatusForwardedRequest
{
    [Key(0)] public string SyncCode { get; set; } = string.Empty;
    [Key(1)] public PairOnlineStatus Status { get; set; }
    [Key(2)] public PairInformation? Information { get; set; }
    [Key(3)] public Nomenclature? Nomenclature { get; set; }

    public UpdateOnlineStatusForwardedRequest()
    {
    }

    public UpdateOnlineStatusForwardedRequest(string syncCode, PairOnlineStatus status, PairInformation? information, Nomenclature? nomenclature)
    {
        SyncCode = syncCode;
        Status = status;
        Information = information;
        Nomenclature = nomenclature;
    }
}