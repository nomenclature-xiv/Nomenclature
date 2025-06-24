using MessagePack;

namespace NomenclatureCommon.Domain;

[MessagePackObject]
public record Character
{
    [Key(0)]
    public string Name { get; set; } = string.Empty;
    
    [Key(1)]
    public string World { get; set; } = string.Empty;

    public Character()
    {
    }

    public Character(string fullname)
    {
        var split =  fullname.Split('@');
        Name = split[0];
        World = split[1];
    }

    public Character(string name, string world)
    {
        Name = name;
        World = world;
    }
    
    public override string ToString()
    {
        return string.Concat(Name, "@", World);
    }
}