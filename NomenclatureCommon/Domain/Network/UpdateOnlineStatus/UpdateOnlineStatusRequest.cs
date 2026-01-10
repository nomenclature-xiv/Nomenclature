using MessagePack;

namespace NomenclatureCommon.Domain.Network.UpdateOnlineStatus;

[MessagePackObject]
public record UpdateOnlineStatusRequest
{
    [Key(0)] public string SyncCode { get; set; } = string.Empty;
    [Key(1)] public PairOnlineStatus Status { get; set; }
    [Key(2)] public PairInformation? Information { get; set; }

    public UpdateOnlineStatusRequest()
    {
    }

    public UpdateOnlineStatusRequest(string syncCode, PairOnlineStatus status, PairInformation? information)
    {
        SyncCode = syncCode;
        Status = status;
        Information = information;
    }
}