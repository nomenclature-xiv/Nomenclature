using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using NomenclatureClient.UI.Views.Login;
using NomenclatureClient.UI.Views.Manage;
using NomenclatureClient.UI.Views.Nomenclature;
using NomenclatureClient.UI.Views.Pairs;
using NomenclatureClient.UI.Views.Register;
using NomenclatureClient.UI.Views.Settings;

namespace NomenclatureClient.UI;

public class PrimaryWindow : Window
{
    private readonly PrimaryWindowController _controller;
    private readonly LoginView _loginView;
    private readonly ManageView _manageView;
    private readonly NomenclatureView _nomenclatureView;
    private readonly PairsView _pairsView;
    private readonly RegisterView _registerView;
    private readonly SettingsView _settingsView;
    
    public PrimaryWindow(
        PrimaryWindowController controller,
        LoginView loginView,
        ManageView manageView,
        NomenclatureView nomenclatureView,
        PairsView pairsView,
        RegisterView registerView,
        SettingsView settingsView) : base($"Nomenclature - Version {Plugin.Version}")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(500),
            MaximumSize = ImGui.GetIO().DisplaySize
        };

        _controller = controller;
        _loginView = loginView;
        _manageView = manageView;
        _nomenclatureView = nomenclatureView;
        _pairsView = pairsView;
        _registerView = registerView;
        _settingsView = settingsView;
    }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("PrimaryTabBar"))
        {
            if (ImGui.BeginTabItem("Login"))
            {
                _loginView.Draw();
                ImGui.EndTabItem();
            }
            
            if (ImGui.BeginTabItem("Register"))
            {
                _registerView.Draw();
                ImGui.EndTabItem();
            }
            
            if (ImGui.BeginTabItem("Manage"))
            {
                _manageView.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Nomenclature"))
            {
                _nomenclatureView.Draw();
                ImGui.EndTabItem();
            }
            
            if (ImGui.BeginTabItem("Pairs"))
            {
                _pairsView.Draw();
                ImGui.EndTabItem();
            }
            
            if (ImGui.BeginTabItem("Settings"))
            {
                _settingsView.Draw();
                ImGui.EndTabItem();
            }
            
            ImGui.EndTabBar();
        }
    }
}