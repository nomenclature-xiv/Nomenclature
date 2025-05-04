using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Microsoft.AspNetCore.SignalR.Client;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureClient.Utils;

namespace NomenclatureClient.UI.New;

public class MainWindow : Window
{
    // Injected
    private readonly CharacterService _characterService;
    private readonly Configuration _configuration;
    private readonly MainWindowController _controller;
    private readonly NetworkHubService _networkHubService;
    private readonly RegistrationWindow _registrationWindow;
    
    // Instantiated
    private string _newName = string.Empty;
    private string _newWorld = string.Empty;
    private bool _shouldChangeName;
    private bool _shouldChangeWorld;

    public MainWindow(
        CharacterService characterService, 
        Configuration configuration,
        MainWindowController controller,
        NetworkHubService networkHubService,
        RegistrationWindow registrationWindow) : base($"Nomenclature - Version {Plugin.Version}")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(280, 400),
            MaximumSize = ImGui.GetIO().DisplaySize
        };

        _characterService = characterService;
        _configuration = configuration;
        _controller = controller;
        _networkHubService = networkHubService;
        _registrationWindow = registrationWindow;
    }

    public override void Draw()
    {
        if (ImGui.BeginChild("##NomenclatureMainWindow", Vector2.Zero, false) is false)
            return;

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (_networkHubService.Connection.State)
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

            SharedUserInterfaces.TextCentered($"{_networkHubService.UserCount} User(s) Online");
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            var dimensions = new Vector2((size.X - padding.X * 3) * 0.5f, 0);
            if(ImGui.Button("Disconnect", dimensions))
            {
                _networkHubService.Disconnect();
            }
            ImGui.SameLine();
            ImGui.Button("Blocklist", dimensions);
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            FontService.MediumFont?.Push();
            ImGui.TextUnformatted("Change Name & World");
            FontService.MediumFont?.Pop();

            ImGui.Checkbox("##EnableNameChange", ref _shouldChangeName);
            ImGui.SameLine();

            if (_shouldChangeName is false)
                ImGui.BeginDisabled();

            ImGui.SetNextItemWidth(size.X - padding.X - ImGui.GetCursorPosX());
            ImGui.InputTextWithHint("##A2", "Name", ref _newName, 100);

            if (_shouldChangeName is false)
                ImGui.EndDisabled();

            ImGui.Checkbox("##EnableWorldChange", ref _shouldChangeWorld);
            ImGui.SameLine();

            if (_shouldChangeWorld is false)
                ImGui.BeginDisabled();

            ImGui.SetNextItemWidth(size.X - padding.X - ImGui.GetCursorPosX());
            ImGui.InputTextWithHint("##A3", "World", ref _newWorld, 100);

            if (_shouldChangeWorld is false)
                ImGui.EndDisabled();
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            FontService.MediumFont?.Push();
            ImGui.TextUnformatted("Appearing as");
            FontService.MediumFont?.Pop();

            var name = _shouldChangeName ? _newName : _characterService.CurrentCharacter?.Name ?? string.Empty;
            var world = _shouldChangeWorld ? _newWorld : _characterService.CurrentCharacter?.World ?? string.Empty;

            ImGui.TextUnformatted($"{name} «{world}»");
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            FontService.MediumFont?.Push();
            if(ImGui.Button("Change Name",
                new Vector2(size.X - padding.X * 2, size.Y - padding.Y - ImGui.GetCursorPosY())))
            {
               _controller.ChangeName(
                    _shouldChangeName ? _newName : _characterService.CurrentCharacter?.Name ?? string.Empty,
                    _shouldChangeWorld ? _newWorld : _characterService.CurrentCharacter?.World ?? string.Empty
                    );
            }
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

            SharedUserInterfaces.TextCentered($"{_networkHubService.Connection.State}");
        });
        
        var dimension = new Vector2(size.X - padding.X * 2, 0);
        if (_characterService.CurrentSecret is null)
        {
            SharedUserInterfaces.ContentBox(() =>
            {
                ImGui.TextWrapped("This character is not registered with nomenclature. Please click \"Register\" to start using the plugin.");
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
                if (ImGui.Checkbox("Auto connect?", ref _configuration.AutoConnect))
                    _configuration.Save();
            });
            
            SharedUserInterfaces.ContentBox(() =>
            {
                if (_networkHubService.Connection.State is HubConnectionState.Disconnected)
                {
                    if (ImGui.Button("Connect", dimension))
                        TryConnect();
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

    private async void TryConnect()
    {
        try
        {
            await _networkHubService.Connect();
        }
        catch (Exception)
        {
            // Ignore
        }
    }
    
    private void Debug()
    {
    }
}
