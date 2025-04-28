using MessagePack;
using NomenclatureCommon.Domain.Api.Base;

namespace NomenclatureCommon.Domain.Api.Server;

[MessagePackObject]
public record SyncNameUpdateSubscriptionsResponse : Response
{
    [Key(1)] Dictionary<string, Nomenclature> newlySubscribedNomenclatures { get; set; } = [];
}