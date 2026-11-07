using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using NomenclatureClient.UI.Views.Pairs;

namespace NomenclatureClient.UI.Windows;

public class PairsWindow : Window
{
    private readonly PairsView _pairsView;
    
    public PairsWindow(PairsView pairsView) : base("Nomenclature - Pairs", ImGuiWindowFlags.NoScrollbar)
    {
        _pairsView = pairsView;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(350, 290),
            MaximumSize = new Vector2(350, int.MaxValue),
        };
    }

    public override void Draw()
    {
        _pairsView.Draw();
    }
}