using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using NomenclatureClient.UI.Views.Manage;
using NomenclatureClient.UI.Views.Settings;

namespace NomenclatureClient.UI.Windows;

public class SettingsWindow : Window
{
    private readonly SettingsView _settingsView;
    private readonly ManageView _manageView;
    
    public SettingsWindow(SettingsView settingsView, ManageView manageView) : base("Nomenclature - Settings")
    {
        _settingsView = settingsView;
        _manageView = manageView;
        
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(450, 290),
        };
    }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("##SettingsTabBar"))
        {
            if (ImGui.BeginTabItem("Settings"))
            {
                _settingsView.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Secrets"))
            {
                _manageView.Draw();
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }
    }
}