using MessagePack;

namespace NomenclatureCommon.Domain.Network.RemoveNomenclature;

[MessagePackObject]
public record RemoveNomenclatureResponse
{
    [Key(0)] public RequestErrorCode ErrorCode { get; set; }

    public RemoveNomenclatureResponse()
    {
    }

    public RemoveNomenclatureResponse(RequestErrorCode errorCode)
    {
        ErrorCode = errorCode;
    }
}