namespace NomenclatureServer.Domain;

public record Account(int Id, string Secret, string SyncCode)
{
    public int Id = Id;
    public string Secret = Secret;
    public string SyncCode = SyncCode;
}