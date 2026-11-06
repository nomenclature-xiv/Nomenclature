namespace NomenclatureServer.Types;

public struct LodestoneValidationResponse
{
    public LodestoneValidationStatus  Status { get; set; }
    public string Secret { get; set; }
}