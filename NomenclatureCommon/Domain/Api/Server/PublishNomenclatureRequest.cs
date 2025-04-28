using MessagePack;

namespace NomenclatureCommon.Domain.Api.Server;

[MessagePackObject]
public record PublishNomenclatureRequest
{
    [Key(0)] public required Nomenclature Nomenclature { get; set; }
}