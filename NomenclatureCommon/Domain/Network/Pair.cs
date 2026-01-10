using MessagePack;

namespace NomenclatureCommon.Domain.Network;

[MessagePackObject]
public record Pair
{
    [Key(0)] public string TargetSyncCode { get; set; } = string.Empty;
    [Key(1)] public PairInformation GrantedToTarget { get; set; } = new();
    [Key(2)] public PairInformation? GrantedByTarget { get; set; }

    public Pair()
    {
    }

    public Pair(string targetSyncCode, PairInformation grantedToTarget, PairInformation? grantedByTarget)
    {
        TargetSyncCode = targetSyncCode;
        GrantedToTarget = grantedToTarget;
        GrantedByTarget = grantedByTarget;
    }
}