using MessagePack;

namespace NomenclatureCommon.Domain.Network.UpdateNomenclature;

[MessagePackObject]
public record UpdateNomenclatureRequest
{
    [Key(0)]
    public string? Name { get; set; } = string.Empty;
    
    [Key(1)]
    public string? World { get; set; } = string.Empty;
    
    [Key(2)]
    public UpdateNomenclatureMode Mode { get; set; }

    public UpdateNomenclatureRequest()
    {
    }
    
    public UpdateNomenclatureRequest(string? name, string? world, UpdateNomenclatureMode mode)
    {
        Name = name;
        World = world;
        Mode = mode;
    }
}