using MessagePack;

namespace NomenclatureCommon.Domain.Api.Controller.Oauth;

[MessagePackObject]
public record ValidateCharacterRegistrationOauth
{
    [Key(0)]
    public string Ticket { get; set; } = string.Empty;
    [Key(1)]
    public required Character Character { get; set; }
}