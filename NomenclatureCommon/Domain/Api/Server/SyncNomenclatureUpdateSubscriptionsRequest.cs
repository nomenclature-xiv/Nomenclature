using MessagePack;

namespace NomenclatureCommon.Domain.Api.Server;

[MessagePackObject]
public record SyncNomenclatureUpdateSubscriptionsRequest
{
    [Key(0)] public string[] CharacterIdentitiesToSubscribeTo { get; set; } = [];
    [Key(1)] public string[] CharacterIdentitiesToUnsubscribeFrom { get; set; } = [];
}