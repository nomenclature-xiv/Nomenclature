using MessagePack;

namespace NomenclatureCommon.Domain;

[MessagePackObject]
public record Nomenclature(
    [property: Key(0)] string Name,
    [property: Key(1)] string World) : IEquatable<Nomenclature>
{
    public override int GetHashCode() => HashCode.Combine(Name, World);
}