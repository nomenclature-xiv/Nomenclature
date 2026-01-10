using MessagePack;

namespace NomenclatureCommon.Domain.Network;

[MessagePackObject]
public record PairInformation
{
    [Key(0)] public bool Paused;
    [Key(1)] public int Permissions;

    public PairInformation()
    {
    }
    
    public PairInformation(bool paused, int permissions)
    {
        Paused = paused;
        Permissions = permissions;
    }
}