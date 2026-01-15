using MessagePack;

namespace NomenclatureCommon.Domain.Network.InitializeSession;

[MessagePackObject]
public record InitializeSessionResponse(
    [property: Key(0)] RequestErrorCode ErrorCode ,
    [property: Key(1)] string SyncCode,
    [property: Key(2)] List<PairDto> Pairs
);
/*
{
    [Key(0)] public RequestErrorCode ErrorCode { get; set; }
    [Key(1)] public string SyncCode { get; set; } = string.Empty;
    [Key(2)] public List<PairDto> Pairs { get; set; } = [];

    public InitializeSessionResponse()
    {
    }

    public InitializeSessionResponse(RequestErrorCode errorCode, string syncCode, List<PairDto> pairs)
    {
        ErrorCode = errorCode;
        SyncCode = syncCode;
        Pairs = pairs;
    }
}
*/