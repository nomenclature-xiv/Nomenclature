using Dalamud.Bindings.ImGui;
using NomenclatureClient.Utils;

namespace NomenclatureClient.UI.Views.Settings;

public class SettingsView(SettingsViewController controller)
{
    public void Draw()
    {
        SharedUserInterfaces.ContentBox(() =>
        {
            if (ImGui.Checkbox("Auto-login", ref controller.AutoLogin))
            {
                controller.UpdateAutologin();
            }
            ImGui.Separator();
            if (ImGui.Button("Disconnect"))
            {
                controller.Disconnect();
            }
        });
    }
}