using MessagePack;

namespace NomenclatureCommon.Domain.Network.InitializeSession;

[MessagePackObject]
public record InitializeSessionResponse
{
    [Key(0)] public RequestErrorCode ErrorCode { get; set; }
    [Key(1)] public string SyncCode { get; set; } = string.Empty;
    [Key(2)] public List<PairRelationship> Relationships { get; set; } = [];

    public InitializeSessionResponse()
    {
    }

    public InitializeSessionResponse(RequestErrorCode errorCode, string syncCode, List<PairRelationship> relationships)
    {
        ErrorCode = errorCode;
        SyncCode = syncCode;
        Relationships = relationships;
    }
}