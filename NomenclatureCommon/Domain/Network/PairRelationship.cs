using MessagePack;

namespace NomenclatureCommon.Domain.Network;

[MessagePackObject]
public record PairRelationship
{   
    [Key(0)] public Pair Pair { get; set; } = new Pair();
    [Key(1)] public PairOnlineStatus Status { get; set; }

    public PairRelationship()
    {
    }

    public PairRelationship(Pair pair, PairOnlineStatus status)
    {
        Pair = pair;
        Status = status;
    }
}