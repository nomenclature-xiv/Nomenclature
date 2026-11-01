using System.Numerics;
using Dalamud.Interface.Windowing;
using NomenclatureClient.UI.Views.Pairs;

namespace NomenclatureClient.UI.Windows;

public class PairsWindow : Window
{
    private readonly PairsView _pairsView;
    
    public PairsWindow(PairsView pairsView) : base("Nomenclature - Pairs")
    {
        _pairsView = pairsView;
        
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(450, 290),
            MaximumSize = new Vector2(450, int.MaxValue),
        };
    }

    public override void Draw()
    {
        _pairsView.Draw();
    }
}