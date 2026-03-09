using MessagePack;

namespace NomenclatureCommon.Domain.Network.Responses;

/// <summary>
///     Response object for all pair-related server operations
/// </summary>
[MessagePackObject]
public record PairResponse
{
    [Key(0)] public PairResponseErrorCode Code;

    public PairResponse()
    {

    }

    public PairResponse(PairResponseErrorCode code)
    {
        Code = code; 
    }
}

/// <summary>
///     Response object for all pair-related server operations
/// </summary>
[MessagePackObject]
public record PairResponse<T>
{
    [Key(0)] public PairResponseErrorCode Code;
    [Key(1)] public T? Value;

    public PairResponse()
    {

    }

    public PairResponse(PairResponseErrorCode Code, T? Value)
    {
        this.Code = Code;
        this.Value = Value;
    }
}