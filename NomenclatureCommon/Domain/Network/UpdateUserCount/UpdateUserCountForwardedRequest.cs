using MessagePack;

namespace NomenclatureCommon.Domain.Network.UpdateUserCount;

[MessagePackObject]
public record UpdateUserCountForwardedRequest
{
    [Key(0)]
    public int UserCount { get; set; }

    public UpdateUserCountForwardedRequest()
    {
    }
    
    public UpdateUserCountForwardedRequest(int userCount)
    {
        UserCount = userCount;
    }
};