using MessagePack;

namespace NomenclatureCommon.Domain.Network.InitializeSession;

[MessagePackObject]
public record InitializeSessionRequest
{
    [Key(0)] public string CharacterName { get; set; } = string.Empty;
    [Key(1)] public string CharacterWorld { get; set; } = string.Empty;
    [Key(2)] public Nomenclature? Nomenclature { get; set; }

    public InitializeSessionRequest()
    {
    }

    public InitializeSessionRequest(string characterName, string characterWorld, Nomenclature? nomenclature)
    {
        CharacterName = characterName;
        CharacterWorld = characterWorld;
        Nomenclature = nomenclature;
    }
}