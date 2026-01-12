using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace NomenclatureClient.UI;

public class PrimaryWindow : Window
{
    public PrimaryWindow() : base($"Nomenclature - Version {Plugin.Version}")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(500),
            MaximumSize = ImGui.GetIO().DisplaySize
        };
    }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("PrimaryTabBar"))
        {
            if (ImGui.BeginTabItem("Login"))
            {
                ImGui.TextUnformatted("A");
                ImGui.EndTabItem();
            }
            
            if (ImGui.BeginTabItem("Register"))
            {
                ImGui.TextUnformatted("B");
                ImGui.EndTabItem();
            }
            
            if (ImGui.BeginTabItem("Manage"))
            {
                ImGui.TextUnformatted("B");
                ImGui.EndTabItem();
            }
            
            if (ImGui.BeginTabItem("Pairs"))
            {
                ImGui.TextUnformatted("B");
                ImGui.EndTabItem();
            }
            
            if (ImGui.BeginTabItem("Options"))
            {
                ImGui.TextUnformatted("B");
                ImGui.EndTabItem();
            }
            
            ImGui.EndTabBar();
        }
    }
}