namespace NomenclatureCommon.Domain.Network.UpdateNomenclature;

[Flags]
public enum UpdateNomenclatureMode
{
    None = 0,
    Name = 1 << 0,
    World = 1 << 1
}