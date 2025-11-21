using System;
using System.Collections.Generic;

namespace NomenclatureClient.Types;

[Serializable]
public class CharacterConfiguration
{
    public bool AutoConnect;
    public bool OverrideName;
    public bool OverrideWorld;
    public string? Name;
    public string? World;

    public CharacterConfiguration()
    {
    }
}