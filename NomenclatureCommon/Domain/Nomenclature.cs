using MessagePack;

namespace NomenclatureCommon.Domain;

[MessagePackObject]
public record Nomenclature
{
    [Key(0)] public string Name { get; set; } = string.Empty;
    [Key(1)] public NomenclatureBehavior NameBehavior { get; set; }
    [Key(2)] public string World { get; set; } = string.Empty;
    [Key(3)] public NomenclatureBehavior WorldBehavior { get; set; }
    
    // TODO: Behavior type for Name and World
    //          Example: Display Original, Override Original, Display Nothing

    public Nomenclature()
    {
    }

    public Nomenclature(string name, NomenclatureBehavior nameBehavior, string world, NomenclatureBehavior worldBehavior)
    {
        Name = name;
        NameBehavior = nameBehavior;
        World = world;
        WorldBehavior = worldBehavior;
    }
}