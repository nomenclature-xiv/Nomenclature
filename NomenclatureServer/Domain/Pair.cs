namespace NomenclatureServer.Domain;

/// <summary>
///     Represents a pair 
/// </summary>
public record Pair(string SyncCode, bool LeftSidePaused, bool? RightSidePaused)
{
    /// <summary>
    ///     The right side's sync code (the left side being the owner of the pair)
    /// </summary>
    public readonly string SyncCode = SyncCode;
    
    /// <summary>
    ///     Did the left side pause the right side? (the left side being the owner of the pair)
    /// </summary>
    public readonly bool LeftSidePaused = LeftSidePaused;
    
    /// <summary>
    ///     Did the right side pause the left side? (the left side being the owner of the pair)
    /// </summary>
    public readonly bool? RightSidePaused = RightSidePaused;

    /// <summary>
    ///     Is this pair a two-way pair, meaning both sides have granted permission to each other
    /// </summary>
    public bool IsOneWay() => RightSidePaused is null;
}