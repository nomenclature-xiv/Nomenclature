namespace NomenclatureServer.Domain;

public enum RegistrationErrorCode
{
    Success,
    NoActiveValidation,
    CharacterNotFound,
    InvalidValidationCode
}