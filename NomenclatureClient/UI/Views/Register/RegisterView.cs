using Dalamud.Bindings.ImGui;
using NomenclatureClient.Services;
using NomenclatureClient.Utils;

namespace NomenclatureClient.UI.Views.Register;

public class RegisterView(RegisterViewController controller)
{
    public void Draw()
    {
        SharedUserInterfaces.ContentBox(() =>
        {
            if (ImGui.Button("Login with XIVAuth"))
                controller.StartRegistration();
        });
    }
}