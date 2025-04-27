using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using NomenclatureClient.Utils;
using NomenclatureClient.Services;
using NomenclatureClient.Types;
using Serilog;
using System;
using Dalamud.Plugin.Services;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Api;
using NomenclatureCommon.Domain.Api.Base;
using NomenclatureCommon.Domain.Api.Server;
using Microsoft.AspNetCore.SignalR.Client;
using NomenclatureClient.Network;

namespace NomenclatureClient.UI;

public class MainWindow : Window
{
    private readonly Configuration Configuration;
    private readonly MainWindowController MainWindowController;
    private readonly NetworkNameService _nameService;
    private readonly NetworkHubService _hubService;
    private readonly WorldService _worldService;
    private readonly IPluginLog _log;
    private readonly RegistrationWindow RegistrationWindow;
    private readonly List<string> _worldNames;

    public MainWindow(IPluginLog log, Configuration configuration, MainWindowController mainWindowController, RegistrationWindow registrationWindow, WorldService worldService, NetworkNameService nameService, NetworkHubService hubService) : base("Nomenclature")
    {
        Configuration = configuration;
        MainWindowController = mainWindowController;
        _log = log;
        RegistrationWindow = registrationWindow;
        _worldService = worldService;
        _hubService = hubService;
        _nameService = nameService;
        _worldNames = _worldService.WorldNames;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 200),
            MaximumSize = ImGui.GetIO().DisplaySize
        };
    }

    public override void Draw()
    {
        if(ImGui.BeginTabBar("Tab Bar##tabbarmain", ImGuiTabBarFlags.None))
        {
            DrawInfoTab();
            if (Configuration.LocalCharacters.Count > 0)
            {
                DrawMainTab();
                DrawBlocklistTab();
                DrawSettingsTab();
            }
            ImGui.EndTabBar();
        }
    }
    private void DrawInfoTab()
    {
        if (ImGui.BeginTabItem("Info"))
        {
            if (Configuration.LocalCharacters.Count == 0)
            {
                ImGui.NewLine();
                SharedUserInterfaces.TextCentered("You haven't registered a character yet!");
                ImGui.NewLine();
                if(SharedUserInterfaces.ButtonCentered("Register"))
                {
                    RegistrationWindow.Toggle();
                }
            }
            else
            {
                if(_hubService.Connection.State == HubConnectionState.Connected)
                {
                    ImGui.NewLine();
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4() { W = 255, X = 0, Y = 255, Z = 0 });
                    SharedUserInterfaces.TextCentered("Connected!");
                    ImGui.PopStyleColor();
                    ImGui.SameLine();
                    if(SharedUserInterfaces.IconButton(FontAwesomeIcon.Unlink, tooltip: "Disconnect"))
                    {
                        _hubService.Disconnect().ConfigureAwait(false);
                    }
                }
                else
                {
                    ImGui.NewLine();
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4() { W = 255, X = 255, Y = 0, Z = 0 });
                    SharedUserInterfaces.TextCentered("Disconnected!");
                    ImGui.PopStyleColor();
                    ImGui.SameLine();
                    if(SharedUserInterfaces.IconButton(FontAwesomeIcon.Link, tooltip: "Connect"))
                    {
                        _hubService.Connect().ConfigureAwait(false);
                    }
                }
            }
            ImGui.EndTabItem();
        }
    }

    private void DrawMainTab()
    {
        if(ImGui.BeginTabItem("Name"))
        {
            try
            {
                if(ImGui.Checkbox("##Enabled", ref MainWindowController.SelfChangeNameEnabled))
                {
                    if(MainWindowController.SelfChangeNameEnabled)
                    {
                        UpdateName();
                    }
                    else
                    {
                        _nameService.ClearName();
                    }
                    Configuration.SelfChangeName = MainWindowController.SelfChangeNameEnabled;
                    Configuration.Save();
                }
                if(ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted("Enables your name to be modified via Nomenclature when checked!");
                    ImGui.EndTooltip();
                }
                ImGui.SameLine();
                ImGui.SetNextItemWidth(200);
                if (ImGui.InputTextWithHint("##NomenclatureName", "Name", ref MainWindowController.ChangedName, 32, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    if (MainWindowController.SelfChangeNameEnabled)
                    {
                        UpdateName();
                    }
                }
                ImGui.SameLine();
                ImGui.SetNextItemWidth(120);
                if(ImGui.InputTextWithHint("##NomenclatureWorld", "World", ref MainWindowController.ChangedWorld, 20, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    if (MainWindowController.SelfChangeNameEnabled)
                    {
                        UpdateName();
                    }
                }

            }
            catch (Exception e)
            {
                Log.Verbose($"{e}");
                throw;
            }
            
            
            
            
            
            ImGui.EndTabItem();
        }
    }

    private void DrawBlocklistTab()
    {
        if (ImGui.BeginTabItem("Blocklist"))
        {
            int cnt = 3;
            if (ImGui.BeginTable("##BlocklistTable", cnt, ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.None, 200);
                ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.None, 120);
                ImGui.TableSetupColumn(" ", ImGuiTableColumnFlags.NoResize, 20);
                ImGui.TableHeadersRow();

                for(int i = 0; i < Configuration.BlocklistCharacters.Count; i++)
                {
                    Character blocklistChar = Configuration.BlocklistCharacters[i];

                    ImGui.PushID(new Guid().ToString());

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
                    ImGui.Text(blocklistChar.Name);

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
                    ImGui.Text(blocklistChar.World);

                    ImGui.TableNextColumn();
                    if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Trash, tooltip: "Delete from blocklist."))
                    {
                        Configuration.BlocklistCharacters.Remove(blocklistChar);
                        Configuration.Save();
                    }
                    ImGui.PopID();
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
                ImGui.InputTextWithHint("##BlocklistName", "Name", ref MainWindowController.BlocklistName, 32);

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
                ImGui.Combo("##BlocklistWorld", ref MainWindowController.BlocklistWorld, _worldNames.ToArray(), _worldNames.Count);
                
                ImGui.TableNextColumn();
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Plus))
                {
                    if (ValidateName(MainWindowController.BlocklistName))
                    {
                        Configuration.BlocklistCharacters.Add(new Character(MainWindowController.BlocklistName, _worldNames[MainWindowController.BlocklistWorld]));
                        Configuration.Save();
                        MainWindowController.BlocklistName = string.Empty;
                        MainWindowController.BlocklistWorld = 0;
                    }
                }
                ImGui.EndTable();
            }
            ImGui.EndTabItem();
        }
    }

    private void DrawSettingsTab()
    {
        if(ImGui.BeginTabItem("Settings"))
        {
            if (ImGui.Checkbox("##AutoConnect", ref Configuration.AutoConnect))
            {
                Configuration.Save();
            }
            ImGui.SameLine();
            ImGui.Text("Automatically connect to the server upon login.");
            ImGui.EndTabItem();
        }
    }

    private bool ValidateName(string name)
    {
        return name != null && name.Length > 0;
    }

    private void UpdateName()
    {
        _nameService.UpdateName(MainWindowController.ChangedName, MainWindowController.ChangedWorld);
    }
}