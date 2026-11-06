using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using NomenclatureClient.Services;
using NomenclatureClient.Utils;

namespace NomenclatureClient.UI.Views.Register;

public class RegisterView(RegisterViewController controller)
{
    public void Draw()
    {
        SharedUserInterfaces.ContentBox(() =>
        {
            ImGui.TextWrapped("Please click this button and follow the instructions as given. Lodestone authentication is offered as an alternative; however, we do encourage you to try XIVAuth.");
            if (ImGui.CollapsingHeader("More information about XIVAuth"))
            {
                ImGui.TextWrapped("XIVAuth is a third party website which securely stores verified accounts. The aim in using it is to ensure that the burden on the user to register each character using their Lodestone page be kept to a minimum by means of centralization.");
            }

            
        });
        SharedUserInterfaces.ContentBox(() =>
        {
            if (ImGui.Button("Register with XIVAuth"))
                controller.StartRegistrationXivAuth();
            if (ImGui.Button("Register with the Lodestone"))
                controller.StartRegistrationLodestone();
        });
        if (controller.ShowLodestoneRegistrationText)
        {
            SharedUserInterfaces.ContentBox(() =>
            {
                ImGui.TextUnformatted($"Your registration code is:");
                ImGui.TextWrapped(controller.RegistrationCode);
                ImGui.SameLine();
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Clipboard))
                {
                    ImGui.SetClipboardText(controller.RegistrationCode);
                }
                ImGui.TextWrapped("Please add it to your lodestone bio and click the button below when that's saved. If the request is unsuccessful, please wait a few minutes for it to propagate. Your Lodestone must be public for this to work.");
                if (ImGui.Button("Check Lodestone registration"))
                {
                    controller.CheckRegistrationLodestone();
                }
            });
        }
    }
}