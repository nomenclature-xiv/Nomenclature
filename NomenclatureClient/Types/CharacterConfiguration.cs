using System;
using System.Collections.Generic;

namespace NomenclatureClient.Types;

[Serializable]
public class CharacterConfiguration
{
    public string Secret = string.Empty;
    
    public bool AutoConnect;
    public bool OverrideName;
    public bool OverrideWorld;
    public string? Name;
    public string? World;
    public readonly HashSet<string> BlockedCharacters = [];

    public CharacterConfiguration()
    {
    }

    public CharacterConfiguration(string secret)
    {
        Secret = secret;
    }
}