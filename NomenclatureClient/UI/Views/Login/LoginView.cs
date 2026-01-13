using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using NomenclatureClient.Services;

namespace NomenclatureClient.UI.Views.Login;

public class LoginView(LoginViewController controller, ConfigurationService configuration)
{
    public void Draw()
    {
        FontService.BigFont?.Push();
        ImGui.TextUnformatted("Nomenclature");
        FontService.BigFont?.Pop();
        
        FontService.MediumFont?.Push();
        ImGui.TextUnformatted("Online Status:");
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, controller.Connected ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed);
        ImGui.Text(controller.Connected ? "Connected" : "Disconnected");
        ImGui.PopStyleColor();
        
        if (ImGui.Button(controller.Connected ? "Disconnect" : "Connect"))
            if (controller.Connected)
                controller.Disconnect();
            else
                controller.Connect();
        
        FontService.MediumFont?.Pop();
        
        ImGui.Separator();

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

        ImGui.Checkbox("Automatically login to Nomenclature", ref controller.AutoLogin);
    }
}