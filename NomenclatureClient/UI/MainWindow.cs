using System;
using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using Microsoft.AspNetCore.SignalR.Client;
using NomenclatureClient.Managers;
using NomenclatureClient.Services;
using NomenclatureClient.Types;
using NomenclatureClient.Utils;
using Dalamud.Interface;

namespace NomenclatureClient.UI;

public class MainWindow : Window
{
    // Injected
    private readonly Configuration _config;
    private readonly MainWindowController _controller;
    private readonly SessionService _sessionService;
    private readonly IdentityManager _identityManager;

    public MainWindow(
        Configuration config,
        SessionService sessionService,
        MainWindowController mainWindowController,
        IdentityManager identityManager) : base($"Nomenclature - Version {Plugin.Version}", ImGuiWindowFlags.NoResize)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(450, 290),
            MaximumSize = new Vector2(450, 290)
        };
        
        _config = config;
        _controller = mainWindowController;
        _sessionService = sessionService;
        _identityManager = identityManager;
    }

    public override void Draw()
    {
        if (ImGui.BeginChild("##NomenclatureMainWindow", Vector2.Zero, false) is false)
            return;

        if (_sessionService.CurrentSession != null)
        {
            DrawConnectedMenu();
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
            string name = _controller.OverrideName ? _controller.OverwrittenName : _sessionService.CurrentSession.Character.Name;
            string world = _controller.OverrideWorld ? _controller.OverwrittenWorld : _sessionService.CurrentSession.Character.World;
            ImGui.Text(String.Concat(
                _identityManager.GetDisplayName(), 
                " → ", 
                name,
                world == string.Empty ? "" : "@",
                world));
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            if (SharedUserInterfaces.IconButton(Dalamud.Interface.FontAwesomeIcon.Undo, new Vector2(50,50), "Reset to default.", FontService.MediumFont))
            {
                _controller.ResetName();
            }
            ImGui.SameLine();
            FontService.MediumFont?.Push();
            if (ImGui.Button("Change Name", new Vector2(360, 50)))
                _controller.ChangeName();
            FontService.MediumFont?.Pop();
        }, false);
    }
}
