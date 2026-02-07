using MessagePack;
using NomenclatureCommon.Domain.Network.Pairs;

namespace NomenclatureCommon.Domain.Network.InitializeSession;

[MessagePackObject]
public record InitializeSessionResponse

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