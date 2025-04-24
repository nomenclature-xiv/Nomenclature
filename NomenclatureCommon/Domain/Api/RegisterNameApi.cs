using MessagePack;

namespace NomenclatureCommon.Domain.Api;

[MessagePackObject(true)]
public record RegisterNameRequest
{
    public string Name { get; set; } = string.Empty;
}

[MessagePackObject(true)]
public record RegisterNameResponse
{
    public bool Success { get; set; }
}