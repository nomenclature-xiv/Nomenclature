using MessagePack;

namespace NomenclatureCommon.Domain;

[MessagePackObject]
public record Nomenclature
{
    [Key(0)]
    public string? Name { get; set; }
    
    [Key(1)]
    public string? World { get; set; }

    public Nomenclature()
    {
    }

    public Nomenclature(string? name, string? world)
    {
        Name = name;
        World = world;
    }
}