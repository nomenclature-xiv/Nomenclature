using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Nomenclature.UI;

public class MainWindow : Window
{
    private readonly Configuration Configuration;
    public MainWindow(Configuration configuration) : base("Nomenclature")
    {
        Configuration = configuration;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 200),
            MaximumSize = ImGui.GetIO().DisplaySize
        };
    }

    public override void Draw()
    {
        if(ImGui.BeginTabBar("Tab Bar##tabbarmain", ImGuiTabBarFlags.None))
        {
            DrawMainTab();
            DrawBlocklistTab();
            DrawSettingsTab();
            ImGui.EndTabBar();
        }
    }

    private void DrawMainTab()
    {
        string name = Configuration.Name;
        if(ImGui.BeginTabItem("Nomenclature"))
        {
            ImGui.Text("Name: ");
            ImGui.SameLine();
            ImGui.InputText("##NomenclatureName", ref name, 32);
            ImGui.EndTabItem();
        }
    }

    private void DrawBlocklistTab()
    {
        if (ImGui.BeginTabItem("Blocklist"))
        {
            ImGui.EndTabItem();
        }
    }

    private void DrawSettingsTab()
    {
        if (ImGui.BeginTabItem("Settings"))
        {
            ImGui.EndTabItem();
        }
    }
}