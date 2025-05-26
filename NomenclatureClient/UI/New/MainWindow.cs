using System;
using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Microsoft.AspNetCore.SignalR.Client;
using NomenclatureClient.Network;
using NomenclatureClient.Services;
using NomenclatureClient.Services.New;
using NomenclatureClient.Utils;

namespace NomenclatureClient.UI.New;

public class MainWindow : Window
{
    // Injected
    private readonly CharacterService _characterService;
    private readonly Configuration _configuration;
    private readonly NameService _nameService;
    private readonly NetworkHubService _networkHubService;
    private readonly RegistrationWindow _registrationWindow;
    private readonly BlocklistWindow _blocklistWindow;
    private readonly MainWindowController _mainWindowController;

    public MainWindow(
        CharacterService characterService,
        Configuration configuration,
        NameService nameService,
        NetworkHubService networkHubService,
        RegistrationWindow registrationWindow,
        BlocklistWindow blocklistWindow,
        MainWindowController mainWindowController) : base($"Nomenclature - Version {Plugin.Version}")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(280, 450),
            MaximumSize = ImGui.GetIO().DisplaySize
        };

        _characterService = characterService;
        _configuration = configuration;
        _nameService = nameService;
        _networkHubService = networkHubService;
        _registrationWindow = registrationWindow;
        _blocklistWindow = blocklistWindow;
        _mainWindowController = mainWindowController;
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

            SharedUserInterfaces.TextCentered($"{_networkHubService.UserCount} Corpse Puppets(s) Online");
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            var dimensions = new Vector2((size.X - padding.X * 3) * 0.5f, 0);
            if(ImGui.Button("Disconnect", dimensions))
            {
                _networkHubService.Disconnect();
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

            if(_mainWindowController.Locked)
                ImGui.BeginDisabled();

            ImGui.Checkbox("##EnableNameChange", ref _characterService.CurrentConfig.UseName);
            ImGui.SameLine();

            if (_characterService.CurrentConfig.UseName is false && !_mainWindowController.Locked)
                ImGui.BeginDisabled();

            ImGui.SetNextItemWidth(size.X - padding.X - ImGui.GetCursorPosX());
            ImGui.InputTextWithHint("##A2", "Name", ref _characterService.CurrentConfig.Name, 100);

            if (_characterService.CurrentConfig.UseName is false && !_mainWindowController.Locked)
                ImGui.EndDisabled();

            ImGui.Checkbox("##EnableWorldChange", ref _characterService.CurrentConfig.UseWorld);
            ImGui.SameLine();

            if (_characterService.CurrentConfig.UseWorld is false && !_mainWindowController.Locked)
                ImGui.BeginDisabled();

            ImGui.SetNextItemWidth(size.X - padding.X - ImGui.GetCursorPosX());
            ImGui.InputTextWithHint("##A3", "World", ref _characterService.CurrentConfig.World, 100);

            if (_characterService.CurrentConfig.UseWorld is false || _mainWindowController.Locked)
                ImGui.EndDisabled();
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            FontService.MediumFont?.Push();
            ImGui.TextUnformatted("Pending changes");
            FontService.MediumFont?.Pop();

            DisplayName(_characterService.CurrentConfig.UseName ? _characterService.CurrentConfig.Name : null, _characterService.CurrentConfig.UseWorld ? _characterService.CurrentConfig.World : null);

            FontService.MediumFont?.Push();
            ImGui.TextUnformatted("Appearing as");
            FontService.MediumFont?.Pop();

            DisplayName(IdentityService.CurrentNomenclature?.Name, IdentityService.CurrentNomenclature?.World);
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            FontService.MediumFont?.Push();
            if(ImGui.Button("Change Name",
                new Vector2(size.X - padding.X * 2, size.Y - padding.Y - ImGui.GetCursorPosY())))
            {
                _nameService.ChangeName();
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
        if (_characterService.CurrentConfig is null)
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

    private void DisplayName(string? name, string? world)
    {
        string outname = name ?? _characterService.CurrentCharacter.Name;
        outname = name is null && world is null ? outname : outname == string.Empty ? outname : String.Concat(outname, "*");
        string outworld = world ?? _characterService.CurrentCharacter.World;
        outworld = outworld == string.Empty ? outworld : string.Concat("«", outworld, "»");

        ImGui.TextUnformatted($"{outname} {outworld}");
    }

    private async void TryConnect()
    {
        try
        {
            await _networkHubService.Connect();
            if(_networkHubService.Connection.State == HubConnectionState.Connected)
                await _nameService.ChangeName();
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
