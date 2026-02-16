using MessagePack;

namespace NomenclatureCommon.Domain.Network.Responses;

/// <summary>
///     Response object for all pair-related server operations
/// </summary>
[MessagePackObject]
public record PairResponse(
    [property: Key(0)] PairResponseErrorCode Code
);

/// <summary>
///     Response object for all pair-related server operations
/// </summary>
[MessagePackObject]
public record PairResponse<T>(
    [property: Key(0)] PairResponseErrorCode Code,
    [property: Key(1)] T? Value
);