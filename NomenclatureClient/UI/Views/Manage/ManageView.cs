using System.Linq;
using Dalamud.Bindings.ImGui;
using NomenclatureClient.Services;

namespace NomenclatureClient.UI.Views.Manage;

public class ManageView(ManageViewController controller, ConfigurationService configuration)
{
    public void Draw()
    {
        FontService.BigFont?.Push();
        ImGui.TextUnformatted("Secret Keys");
        FontService.BigFont?.Pop();

        ImGui.Checkbox("Show Secrets", ref controller.ShowSecrets);
        
        ImGui.Separator();
        
        if (configuration.Configuration.Secrets.Count is 0)
        {
            ImGui.TextWrapped("You do not have any registered secrets, visit the register tab to get one.");
        }
        else
        {
            var secrets = configuration.Configuration.Secrets.ToDictionary();
            
            ImGui.TextWrapped($"Displaying {secrets.Count} secret{(secrets.Count is 1 ? string.Empty : 's')}.");
            
            foreach (var (identifier, secret) in secrets)
            {
                var i = identifier;
                var s = secret;
                ImGui.InputText("Secret Identifier", ref i, 512, ImGuiInputTextFlags.ReadOnly);
                ImGui.InputText("Secret Key", ref s, 512, controller.ShowSecrets ? ImGuiInputTextFlags.ReadOnly : ImGuiInputTextFlags.ReadOnly | ImGuiInputTextFlags.Password);
                if (ImGui.Button($"Delete Key##{secret}"))
                    controller.DeleteSecret(identifier);
            }
        }
    }
}