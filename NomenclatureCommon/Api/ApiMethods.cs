using MessagePack;

namespace NomenclatureCommon.Api;

public static class ApiMethods
{
    public const string ClearName = "ClearName";
    public const string SetName = "SetName";
    public const string RegisterCharacterInitiate = "RegisterCharacterInitiate";
    public const string RegisterCharacterValidate = "RegisterCharacterValidate";
    public const string QueryChangedNames = "QueryChangedNames";
}

// Server

[MessagePackObject]
public record GenericResponse
{
    [Key(0)]
    public bool Success { get; set; }
}

[MessagePackObject]
public record ClearNameRequest;

[MessagePackObject]
public record NewNameRequest
{
    [Key(0)]
    public string Name { get; set; } = string.Empty;
}

[MessagePackObject]
public record QueryChangedNamesRequest
{
    [Key(0)]
    public string[] NamesToQuery { get; set; } = [];
}

[MessagePackObject]
public record QueryChangedNamesResponse
{
    [Key(0)]
    public Dictionary<string, string> ModifiedNames { get; set; } = [];
}

// Controller

public record RegisterCharacterInitiateRequest
{
    public string CharacterName { get; set; } = string.Empty;
    public string WorldName { get; set; } = string.Empty;
}

public record RegisterCharacterValidateRequest
{
    public string CharacterName { get; set; } = string.Empty;
    public string ValidationCode { get; set; } = string.Empty;
}

public record TokenRequest
{
    public string Secret { get; set; } = string.Empty;
}