using MessagePack;

namespace NomenclatureCommon.Domain;

[MessagePackObject]
public record Character(string Name, string World) : IEquatable<Character>
{
    public override int GetHashCode() => HashCode.Combine(Name, World);
}