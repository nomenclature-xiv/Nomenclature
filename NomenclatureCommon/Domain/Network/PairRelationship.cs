using MessagePack;

namespace NomenclatureCommon.Domain.Network;

[MessagePackObject]
public record PairRelationship
{   
    [Key(0)] public Pair Pair { get; set; } = new Pair();
    [Key(1)] public PairOnlineStatus Status { get; set; }
    [Key(2)] public Nomenclature? Nomenclature { get; set; }

    public PairRelationship()
    {
    }

    public PairRelationship(Pair pair, PairOnlineStatus status, Nomenclature? nomenclature)
    {
        Pair = pair;
        Status = status;
        Nomenclature = nomenclature;
    }
}