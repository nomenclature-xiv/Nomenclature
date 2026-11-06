using MessagePack;

namespace NomenclatureCommon.Domain.Api.Controller.Lodestone;

[MessagePackObject]
public record BeginCharacterRegistrationResponseLodestone
{
    [Key(0)] public bool Success { get; set; }
    [Key(1)]
    public string Code { get; set; } = string.Empty;

    public BeginCharacterRegistrationResponseLodestone() { }
    public BeginCharacterRegistrationResponseLodestone(bool success, string code)
    {
        Success = success;
        Code = code;
    }
}