using MessagePack;

namespace NomenclatureCommon.Domain.Network.UpdateNomenclature;

[MessagePackObject]
public record UpdateNomenclatureForwardedRequest
{
    [Key(0)]
    public string CharacterName {  get; set; } = string.Empty;
    
    [Key(1)]
    public Nomenclature Nomenclature { get; set; } = new();

    public UpdateNomenclatureForwardedRequest()
    {
    }

    public UpdateNomenclatureForwardedRequest(string characterName, Nomenclature nomenclature)
    {
        CharacterName = characterName;
        Nomenclature = nomenclature;
    }
}