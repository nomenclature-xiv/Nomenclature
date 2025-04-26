namespace NomenclatureCommon.Domain.Api.Controller;

public record BeginCharacterRegistrationRequest
{
    public Character Character { get; set; } = new();
}