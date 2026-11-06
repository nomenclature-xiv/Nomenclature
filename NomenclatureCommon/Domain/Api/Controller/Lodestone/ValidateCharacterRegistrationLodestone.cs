using MessagePack;

namespace NomenclatureCommon.Domain.Api.Controller.Lodestone;

[MessagePackObject]
public record ValidateCharacterRegistrationLodestone
{
    [Key(0)]
    public string Code { get; set; } = string.Empty;
}