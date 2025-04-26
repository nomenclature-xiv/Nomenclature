namespace NomenclatureCommon.Domain.Api.Controller;

public record ValidateCharacterRegistration
{
    public string ValidationCode { get; set; } = string.Empty;
}