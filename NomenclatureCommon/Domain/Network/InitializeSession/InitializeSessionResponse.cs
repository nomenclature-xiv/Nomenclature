using MessagePack;

namespace NomenclatureCommon.Domain.Network.InitializeSession;

[MessagePackObject]
public record InitializeSessionResponse
{
    [Key(0)] public string SyncCode { get; set; } = string.Empty;
    [Key(1)] public List<PairRelationship> Relationships { get; set; } = [];

    public InitializeSessionResponse()
    {
    }

    public InitializeSessionResponse(string syncCode, List<PairRelationship> relationships)
    {
        SyncCode = syncCode;
        Relationships = relationships;
    }
}