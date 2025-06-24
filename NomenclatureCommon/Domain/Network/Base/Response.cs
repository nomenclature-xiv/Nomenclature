using MessagePack;

namespace NomenclatureCommon.Domain.Network.Base;

[MessagePackObject]
public record Response
{
    [Key(0)]
    public bool Success { get; set; }

    public Response()
    {
    }

    public Response(bool success)
    {
        Success = success;
    }
}