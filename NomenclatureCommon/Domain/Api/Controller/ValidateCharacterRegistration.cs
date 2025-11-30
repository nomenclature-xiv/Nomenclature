using MessagePack;

namespace NomenclatureCommon.Domain.Api.Controller;

[MessagePackObject]
public record ValidateCharacterRegistration
{
    [Key(0)]
    public string Ticket { get; set; } = string.Empty;
    [Key(1)]
    public required Character Character { get; set; }
}