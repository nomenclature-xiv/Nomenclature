using MessagePack;

namespace NomenclatureCommon.Domain.Api.Server;

[MessagePackObject]
public record SyncNomenclatureUpdateSubscriptionsRequest
{
    [Key(0)] public List<string> CharacterIdentitiesToSubscribeTo { get; set; } = [];
    [Key(1)] public List<string> CharacterIdentitiesToUnsubscribeFrom { get; set; } = [];
}