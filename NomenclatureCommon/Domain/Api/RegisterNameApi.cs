using MessagePack;

namespace NomenclatureCommon.Domain.Api;

[MessagePackObject]
public record RegisterNameRequest
{
    [Key(0)]
    public string Name { get; set; } = string.Empty;
}

[MessagePackObject]
public record RegisterNameResponse
{
    [Key(0)]
    public bool Success { get; set; } = true;
}