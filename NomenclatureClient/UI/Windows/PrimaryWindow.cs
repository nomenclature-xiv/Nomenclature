using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using NomenclatureClient.Services;
using NomenclatureClient.UI.Views.Login;
using NomenclatureClient.UI.Views.Manage;
using NomenclatureClient.UI.Views.Nomenclature;
using NomenclatureClient.UI.Views.Pairs;
using NomenclatureClient.UI.Views.Register;
using NomenclatureClient.UI.Views.Settings;
using NomenclatureClient.Utils;
using System;
using System.Numerics;

namespace NomenclatureClient.UI.Windows;

public class PrimaryWindow : Window
{
    private readonly PrimaryWindowController _controller;
    private readonly LoginView _loginView;
    private readonly NomenclatureView _nomenclatureView;
    private readonly RegisterView _registerView;
    
    public PrimaryWindow(
        PrimaryWindowController controller,
        LoginView loginView,
        NomenclatureView nomenclatureView,
        RegisterView registerView) : base($"Nomenclature - Version {Plugin.Version}")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(450, 290),
            MaximumSize = new Vector2(450, 290),
        };

        _controller = controller;
        _loginView = loginView;
        _nomenclatureView = nomenclatureView;
        _registerView = registerView;
    }

    public override void Draw()
    {
        if (!_controller.CharacterRegistered)
        {
            _registerView.Draw();
            return;
        }
        else if (_controller.IsConnected)
        {
           _nomenclatureView.Draw();
        }
        else
        {
            _loginView.Draw();
        }
    }
}