using MessagePack;

namespace NomenclatureCommon.Domain;

[MessagePackObject]
public readonly struct Character(string name, string world) : IEquatable<Character>
{
    [Key(0)] public string Name { get; } = name;

    [Key(1)] public string World { get; } = world;

    public bool Equals(Character other) => Name == other.Name && World == other.World;
    public override bool Equals(object? other) => other is Character character && Equals(character);
    public override int GetHashCode() => HashCode.Combine(Name, World);
}