namespace NomenclatureServer.Domain;

/// <summary>
///     Represents a pair 
/// </summary>
public record Pair(string SyncCode, bool IsOneWay)
{
    /// <summary>
    ///     The right side's sync code (the left side being the owner of the pair)
    /// </summary>
    public readonly string SyncCode = SyncCode;

    /// <summary>
    ///     Is this pair a two-way pair, meaning both sides have granted permission to each other
    /// </summary>
    public bool IsOneWay = IsOneWay;
}