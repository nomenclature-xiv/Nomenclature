namespace NomenclatureCommon.Domain.Api.Controller.Lodestone;

public record BeginCharacterRegistrationRequest
{
    public required Character Character { get; set; }
}