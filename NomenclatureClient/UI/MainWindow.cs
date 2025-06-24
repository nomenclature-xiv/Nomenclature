using System;
using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Microsoft.AspNetCore.SignalR.Client;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureClient.Types;
using NomenclatureClient.Utils;

namespace NomenclatureClient.UI;

public class MainWindow : Window
{
    // Injected
    private readonly Configuration _config;
    private readonly NetworkService _networkService;
    private readonly RegistrationWindow _registrationWindow;
    private readonly BlocklistWindow _blocklistWindow;
    private readonly MainWindowController _controller;
    private readonly SessionService _sessionService;

    public MainWindow(
        Configuration config,
        SessionService sessionService,
        NetworkService networkService,
        RegistrationWindow registrationWindow,
        BlocklistWindow blocklistWindow,
        MainWindowController mainWindowController) : base($"Nomenclature - Version {Plugin.Version}")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(280, 450),
            MaximumSize = ImGui.GetIO().DisplaySize
        };
        
        _config = config;
        _networkService = networkService;
        _registrationWindow = registrationWindow;
        _blocklistWindow = blocklistWindow;
        _controller = mainWindowController;
        _sessionService = sessionService;
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
            FontService.BigFont?.Push();
            SharedUserInterfaces.TextCentered("Nomenclature");
            FontService.BigFont?.Pop();

            SharedUserInterfaces.TextCentered($"{_networkService.UserCount} User(s) Online");
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            var dimensions = new Vector2((size.X - padding.X * 3) * 0.5f, 0);
            if(ImGui.Button("Disconnect", dimensions))
            {
                _controller.Disconnect();
            }
            ImGui.SameLine();
            if(ImGui.Button("Blocklist", dimensions))
            {
                _blocklistWindow.Toggle();
            }
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            FontService.MediumFont?.Push();
            ImGui.TextUnformatted("Change Name & World");
            FontService.MediumFont?.Pop();
            
            if(ImGui.Checkbox("##OverrideNameCheckbox", ref _controller.OverrideName))
                _sessionService.Save();
            ImGui.SameLine();
            
            SharedUserInterfaces.DisableIf(_controller.OverrideName is false, () =>
            {
                ImGui.SetNextItemWidth(size.X - padding.X - ImGui.GetCursorPosX());
                ImGui.InputTextWithHint("##OverrideNameInput", "Name", ref _controller.OverwrittenName, 32);
            });
            
            if(ImGui.Checkbox("##OverrideWorldCheckbox", ref _controller.OverrideWorld))
                _sessionService.Save();
            ImGui.SameLine();
            
            SharedUserInterfaces.DisableIf(_controller.OverrideWorld is false, () =>
            {
                ImGui.SetNextItemWidth(size.X - padding.X - ImGui.GetCursorPosX());
                ImGui.InputTextWithHint("##OverrideWorldInput", "World", ref _controller.OverwrittenWorld, 32);
            });
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            // TODO
            ImGui.Text("TODO");
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            FontService.MediumFont?.Push();
            if(ImGui.Button("Change Name", new Vector2(size.X - padding.X * 2, size.Y - padding.Y - ImGui.GetCursorPosY())))
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
        
        var dimension = new Vector2(size.X - padding.X * 2, 0);
        if (_sessionService.CurrentSession.Character.Name == string.Empty) // Can check any value here, name is easiest since it should always be set
        {
            SharedUserInterfaces.ContentBox(() =>
            {
                ImGui.TextWrapped("This character is not registered with nomenclature. Please click \"Register\" to start using the plugin.");
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
                ImGui.TextWrapped("Please ensure that your character lodestone profile is not private and that you can view it as not-privated BEFORE clicking Register.");
                ImGui.PopStyleColor();
            });
            
            SharedUserInterfaces.ContentBox(() =>
            {
                if (ImGui.Button("Register", dimension))
                    _registrationWindow.IsOpen = true;
            });
        }
        else
        {
            SharedUserInterfaces.ContentBox(() =>
            {
                if (ImGui.Checkbox("Auto connect?", ref _sessionService.CurrentSession.CharacterConfiguration.AutoConnect))
                    _sessionService.Save();
            });
            
            SharedUserInterfaces.ContentBox(() =>
            {
                if (_networkService.Connection.State is HubConnectionState.Disconnected)
                {
                    if (ImGui.Button("Connect", dimension))
                        _controller.TryConnect();
                }
                else
                {
                    ImGui.BeginDisabled();
                    ImGui.Button("Connect", dimension);
                    ImGui.EndDisabled();
                }
            });
        }
    }
}
