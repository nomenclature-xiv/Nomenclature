using MessagePack;

namespace NomenclatureCommon.Domain.Api;

[MessagePackObject(true)]
public record QueryChangedNamesRequest
{
    /// <summary>
    ///     The names of the in-game characters you would like to query
    /// </summary>
    public List<string> NamesToQuery { get; set; } = [];
}

[MessagePackObject(true)]
public record QueryChangedNamesResponse
{
    /// <summary>
    ///     The names of all modified character's with Nomenclature will be added here that were present in the above function
    /// </summary>
    public Dictionary<string, string> ModifiedNames { get; set; } = [];
}