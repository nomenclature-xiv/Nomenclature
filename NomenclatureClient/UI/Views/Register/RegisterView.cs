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
            ImGui.TextUnformatted("Please click this button and follow the instructions as given.");
            if (ImGui.CollapsingHeader("More information about XIVAuth"))
            {
                ImGui.TextWrapped("XIVAuth is a third party website which securely stores verified accounts. The aim in using it is to ensure that the burden on the user to register each character using their Lodestone page be kept to a minimum by means of centralization.");
            }
            if (ImGui.Button("Register with XIVAuth"))
                controller.StartRegistration();
        });
    }
}