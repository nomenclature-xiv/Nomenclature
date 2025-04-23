using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Nomenclature.UI;

public class MainWindow : Window
{
    public MainWindow() : base("Nomenclature")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300),
            MaximumSize = ImGui.GetIO().DisplaySize
        };
    }

    public override void Draw()
    {
        
    }
}