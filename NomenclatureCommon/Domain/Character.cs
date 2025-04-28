using MessagePack;

namespace NomenclatureCommon.Domain;

[MessagePackObject]
public record Character(
    [property: Key(0)] string Name, 
    [property: Key(1)] string World) : IEquatable<Character>
{
    public override int GetHashCode() => HashCode.Combine(Name, World);
    public override string ToString()
    {
        return $"{Name}@{World}";
    }
    public static Character FromString(string character)
    {
        var charworld = character.Split("@");
        return new Character(charworld[0], charworld[1]);
    }
}