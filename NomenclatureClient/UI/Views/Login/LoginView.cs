using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using NomenclatureClient.Services;
using System.Numerics;
using NomenclatureClient.Utils;

namespace NomenclatureClient.UI.Views.Login;

public class LoginView(LoginViewController controller, ConfigurationService configuration)
{
    public void Draw()
    {
        SharedUserInterfaces.ContentBox(() =>
        {
            FontService.MediumFont?.Push();
            if (ImGui.Button("Connect"))
                    controller.Connect();
            FontService.MediumFont?.Pop();
        });

        SharedUserInterfaces.ContentBox(() =>
        {

            FontService.BigFont?.Push();
            ImGui.TextUnformatted("Your Current Character");
            FontService.BigFont?.Pop();

            var name = configuration.CharacterConfiguration?.Name ?? string.Empty;
            var world = configuration.CharacterConfiguration?.World ?? string.Empty;
            var dummy = 0;

            ImGui.BeginDisabled();
            ImGui.InputText("Character Name##CharacterInputText", ref name);
            ImGui.Combo("World##CharacterInputText", ref dummy, world);
            ImGui.EndDisabled();

            if (ImGui.BeginCombo("##SecretPicker", controller.SecretId))
            {
                foreach (var identifier in configuration.Configuration.Secrets.Keys)
                    if (ImGui.Selectable(identifier))
                        controller.UpdateSecretId(identifier);

                ImGui.EndCombo();
            }

            if (ImGui.Checkbox("Automatically login to Nomenclature", ref controller.AutoLogin))
            {
                controller.UpdateAutologin();
            }
        });

       ImGui.TextColored(ImGuiColors.DalamudRed, controller.ErrorMessage);
    }
}