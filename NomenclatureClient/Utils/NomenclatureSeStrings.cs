using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

// ReSharper disable StringLiteralTypo

namespace NomenclatureClient.Utils;

public static class NomenclatureSeStrings
{
    private static readonly Payload ErrorColorPayload = new UIForegroundPayload(17);
    private static readonly Payload SuccessColorPayload = new UIForegroundPayload(45);
    private static readonly Payload NomenclaturePayload = new TextPayload("[Nomenclature]");
    
    public static readonly SeString SetError = new(ErrorColorPayload, NomenclaturePayload, UIForegroundPayload.UIForegroundOff, new TextPayload("Invalid arguments for setboth. Example usage (must include quotes): /nom setboth \"My Name\" \"My World\""));
    public static readonly SeString SetSuccess = new(SuccessColorPayload, NomenclaturePayload, UIForegroundPayload.UIForegroundOff, new TextPayload("Successfully set name and world."));
    
    public static readonly SeString SetNameError = new(ErrorColorPayload, NomenclaturePayload, UIForegroundPayload.UIForegroundOff, new TextPayload("Invalid arugments for setname. Example usage: /nom setname My Name Here"));
    public static readonly SeString SetNameSuccess = new(SuccessColorPayload, NomenclaturePayload, UIForegroundPayload.UIForegroundOff, new TextPayload("Successfully set name."));
    
    public static readonly SeString SetWorldError = new(ErrorColorPayload, NomenclaturePayload, UIForegroundPayload.UIForegroundOff, new TextPayload("Invalid arugments for setworld. Example usage: /nom setworld My World Here"));
    public static readonly SeString SetWorldSuccess = new(SuccessColorPayload, NomenclaturePayload, UIForegroundPayload.UIForegroundOff, new TextPayload("Successfully set world."));

    public static readonly SeString HideSuccess = new(SuccessColorPayload, NomenclaturePayload, UIForegroundPayload.UIForegroundOff, new TextPayload("Successfully hid name and world."));
    public static readonly SeString HideNameSuccess = new(SuccessColorPayload, NomenclaturePayload, UIForegroundPayload.UIForegroundOff, new TextPayload("Successfully hid name."));
    public static readonly SeString HideWorldSuccess = new(SuccessColorPayload, NomenclaturePayload, UIForegroundPayload.UIForegroundOff, new TextPayload("Successfully hid world."));
    
    public static readonly SeString ClearSuccess = new(SuccessColorPayload, NomenclaturePayload, UIForegroundPayload.UIForegroundOff, new TextPayload("Successfully cleared name and world."));
    public static readonly SeString ClearNameSuccess = new(SuccessColorPayload, NomenclaturePayload, UIForegroundPayload.UIForegroundOff, new TextPayload("Successfully cleared name."));
    public static readonly SeString ClearWorldSuccess = new(SuccessColorPayload, NomenclaturePayload, UIForegroundPayload.UIForegroundOff, new TextPayload("Successfully cleared world."));
}