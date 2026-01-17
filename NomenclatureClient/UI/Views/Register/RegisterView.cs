using Dalamud.Bindings.ImGui;
using NomenclatureClient.Services;

namespace NomenclatureClient.UI.Views.Register;

public class RegisterView(RegisterViewController controller)
{
    public void Draw()
    {
        FontService.BigFont?.Push();
        ImGui.TextUnformatted("Temporary Registration (Local)");
        FontService.BigFont?.Pop();
        
        ImGui.InputTextWithHint("Secret Identifier##SecretIdentifierInputText", "Enter secret identifier", ref controller.SecretIdentifier);
        ImGui.InputTextWithHint("Secret Key##SecretKeyInputText", "Enter secret key", ref controller.SecretKey);

        if (ImGui.Button("Add Temporary"))
            controller.AddTemporary();
    }
}