using MessagePack;
using NomenclatureCommon.Domain.Api.Base;

namespace NomenclatureCommon.Domain.Api.Server;

[MessagePackObject]
public record SyncNomenclatureUpdateSubscriptionsResponse : Response
{
    [Key(1)] public Dictionary<string, Nomenclature> NewlySubscribedNomenclatures { get; set; } = [];
}