using MessagePack;

namespace NomenclatureCommon.Domain.Network.UpdateNomenclature;

[MessagePackObject]
public record UpdateNomenclatureResponse
{
    [Key(0)] public RequestErrorCode ErrorCode { get; set; }

    public UpdateNomenclatureResponse()
    {
    }

    public UpdateNomenclatureResponse(RequestErrorCode errorCode)
    {
        ErrorCode = errorCode;
    }
}