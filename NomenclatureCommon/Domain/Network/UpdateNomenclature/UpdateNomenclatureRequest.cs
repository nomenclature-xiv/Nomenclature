using MessagePack;

namespace NomenclatureCommon.Domain.Network.UpdateNomenclature;

[MessagePackObject]
public record UpdateNomenclatureRequest
{
    [Key(0)] public Nomenclature Nomenclature { get; set; } = new();

    public UpdateNomenclatureRequest()
    {
    }

    public UpdateNomenclatureRequest(Nomenclature nomenclature)
    {
        Nomenclature = nomenclature;
    }
}