using MessagePack;

namespace NomenclatureCommon.Domain.Api.Base;

[MessagePackObject]
public record Response
{
    [Key(0)]
    public bool Success { get; set; }
}