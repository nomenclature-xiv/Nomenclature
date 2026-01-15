namespace NomenclatureCommon.Domain.Network;

public enum RequestErrorCode
{
    Uninitialized,
    NotAuthenticatedOrOnline,
    InvalidNomenclature,
    Success,
    Unknown
}