using System;
using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using Microsoft.AspNetCore.SignalR.Client;
using NomenclatureClient.Managers;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureClient.Types;
using NomenclatureClient.Utils;
using Dalamud.Interface;

namespace NomenclatureClient.UI;

public class MainWindow : Window
{
    private static readonly Vector2 SquareButtonSize = new(50);
    private static readonly Vector2 ChangeNameButtonSize = SquareButtonSize with { X = 360 };
    
    // Injected
    private readonly NetworkService _networkService;
    private readonly SettingsWindow _settingsWindow;
    private readonly MainWindowController _controller;
    private readonly IdentityManager _identityManager;
    private readonly ConfigurationService _configuration;

    public MainWindow(
        NetworkService networkService,
        SettingsWindow settingsWindow,
        MainWindowController mainWindowController,
        IdentityManager identityManager,
        ConfigurationService configuration) : base($"Nomenclature - Version {Plugin.Version}", ImGuiWindowFlags.NoResize)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(450, 290),
            MaximumSize = new Vector2(450, 290)
        };
        
        _networkService = networkService;
        _settingsWindow = settingsWindow;
        _controller = mainWindowController;
        _identityManager = identityManager;
        _configuration = configuration;
    }

    public override void Draw()
    {
        if (ImGui.BeginChild("##NomenclatureMainWindow", Vector2.Zero, false) is false)
            return;

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (_networkService.Connection.State)
        {
            case HubConnectionState.Connected:
                DrawConnectedMenu();
                break;

            default:
                DrawDisconnectedMenu();
                break;
        }

        ImGui.EndChild();
    }

    /// <summary>
    ///     Only call this inside an ImGui.Child block
    /// </summary>
    private void DrawConnectedMenu()
    {
        var padding = ImGui.GetStyle().WindowPadding;
        var size = ImGui.GetWindowSize();

        SharedUserInterfaces.ContentBox(() =>
        {
            SharedUserInterfaces.TextCentered("I am");
            ImGui.SameLine(size.X - padding.X * 3 - ImGui.GetFontSize());
            if(SharedUserInterfaces.IconButton(FontAwesomeIcon.Cog, tooltip: "Settings"))
            {
                _settingsWindow.Toggle();
            }
            FontService.MediumFont?.Push();
            SharedUserInterfaces.TextCentered(_identityManager.GetDisplayName());
            FontService.MediumFont?.Pop();
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            ImGui.BeginGroup();
            ImGui.Checkbox("##OverrideNameCheckbox", ref _controller.OverrideName);
            ImGui.SameLine();
            ImGui.TextUnformatted("Override Name");
            SharedUserInterfaces.DisableIf(_controller.OverrideName is false, () =>
            {
                ImGui.SetNextItemWidth(195);
                ImGui.InputTextWithHint("##OverrideNameInput", "Name", ref _controller.OverwrittenName, 32);
            });
            ImGui.EndGroup();

            ImGui.SameLine();

            ImGui.BeginGroup();
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetFontSize() + padding.Y);
            ImGui.TextUnformatted("@");
            ImGui.EndGroup();

            ImGui.SameLine();

            ImGui.BeginGroup();
            ImGui.Checkbox("##OverrideWorldCheckbox", ref _controller.OverrideWorld);
            ImGui.SameLine();
            ImGui.TextUnformatted("Override World");
            SharedUserInterfaces.DisableIf(_controller.OverrideWorld is false, () =>
            {
                ImGui.SetNextItemWidth(195);
                ImGui.InputTextWithHint("##OverrideWorldInput", "World", ref _controller.OverwrittenWorld, 32);
            });
            ImGui.EndGroup();
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            if (_configuration.CharacterConfiguration is null)
                return;
            
            var name = _controller.OverrideName ? _controller.OverwrittenName : _configuration.CharacterConfiguration.Name;
            var world = _controller.OverrideWorld ? _controller.OverwrittenWorld : _configuration.CharacterConfiguration.World;
            ImGui.Text(string.Concat(_identityManager.GetDisplayName(), " → ", name, world == string.Empty ? "" : "@", world));
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Undo, SquareButtonSize, "Reset to default.", FontService.MediumFont))
            {
                _controller.ResetName();
            }
            ImGui.SameLine();
            FontService.MediumFont?.Push();
            if (ImGui.Button("Change Name", ChangeNameButtonSize))
                _controller.ChangeName();
            FontService.MediumFont?.Pop();
        }, false);
    }

    /// <summary>
    ///     Only call this inside an ImGui.Child block
    /// </summary>
    private void DrawDisconnectedMenu()
    {
        var padding = ImGui.GetStyle().WindowPadding;
        var size = ImGui.GetWindowSize();

        SharedUserInterfaces.ContentBox(() =>
        {
            FontService.BigFont?.Push();
            SharedUserInterfaces.TextCentered("Nomenclature");
            FontService.BigFont?.Pop();

            SharedUserInterfaces.TextCentered($"{_networkService.Connection.State}");
        });
        
        // TODO: Remake now that new secret system is in place
    }
}
