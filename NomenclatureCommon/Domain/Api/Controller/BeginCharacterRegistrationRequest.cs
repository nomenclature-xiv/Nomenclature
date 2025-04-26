namespace NomenclatureCommon.Domain.Api.Controller;

public record BeginCharacterRegistrationRequest
{
    public required Character Character { get; set; }
}