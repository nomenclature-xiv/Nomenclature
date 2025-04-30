using System.Numerics;
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
    private readonly NetworkHubService _networkHubService;

    // Instantiated
    private string _newName = string.Empty;
    private string _newWorld = string.Empty;
    private bool _shouldChangeName;
    private bool _shouldChangeWorld;
    
    public MainWindow(CharacterService characterService, Configuration configuration,
        NetworkHubService networkHubService) : base("Nomenclature")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(280, 400),
            MaximumSize = ImGui.GetIO().DisplaySize
        };

        _characterService = characterService;
        _configuration = configuration;
        _networkHubService = networkHubService;
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
                //DrawConnectedMenu();
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
            
            SharedUserInterfaces.TextCentered("0 Corpse Puppets Online");
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            var dimensions = new Vector2((size.X - padding.X * 3) * 0.5f, 0);
            ImGui.Button("Disconnect", dimensions);
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
            ImGui.Button("Change Name",
                new Vector2(size.X - padding.X * 2, size.Y - padding.Y - ImGui.GetCursorPosY()));
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

        SharedUserInterfaces.ContentBox(() =>
        {
            // TODO: Change these buttons to only show connect if the client has a secret for this character, or register instead
            
            var dimension = new Vector2((size.X - padding.X * 3) * 0.5f, 0);
            ImGui.Button("Connect", dimension);
            ImGui.SameLine();
            ImGui.Button("Register", dimension);
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            if (ImGui.Checkbox("Auto connect?", ref _configuration.AutoConnect))
                _configuration.Save();
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            ImGui.TextWrapped(
                "If you are a new character, please click the \"Register\" to start using the plugin.");
        });
    }
}

public class MainWindowController2
{
    
}