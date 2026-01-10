namespace NomenclatureServer.Domain;

public record Account(string Secret, string SyncCode)
{
    public string Secret = Secret;
    public string SyncCode = SyncCode;
}