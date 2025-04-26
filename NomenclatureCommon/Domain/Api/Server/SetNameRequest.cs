using MessagePack;

namespace NomenclatureCommon.Domain.Api.Server;

[MessagePackObject]
public record SetNameRequest
{
    [Key(0)] public Character Nomenclature { get; set; } = new();
}