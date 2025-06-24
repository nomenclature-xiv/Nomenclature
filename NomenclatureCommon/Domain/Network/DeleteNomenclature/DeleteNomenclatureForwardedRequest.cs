using MessagePack;

namespace NomenclatureCommon.Domain.Network.DeleteNomenclature;

[MessagePackObject]
public record DeleteNomenclatureForwardedRequest
{
    [Key(0)]
    public string CharacterName { get; set; } = string.Empty;

    public DeleteNomenclatureForwardedRequest()
    {
    }

    public DeleteNomenclatureForwardedRequest(string characterName)
    {
        CharacterName = characterName;
    }
}