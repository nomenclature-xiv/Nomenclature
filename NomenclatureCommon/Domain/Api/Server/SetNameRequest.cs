using MessagePack;
using NomenclatureCommon.Domain.Api.Types;

namespace NomenclatureCommon.Domain.Api.Server;

[MessagePackObject]
public record SetNameRequest
{
    [Key(0)] public required Character Nomenclature { get; set; }
    [Key(1)] public ChangeItems ChangeItems { get; set; }
}