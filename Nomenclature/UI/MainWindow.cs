using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Nomenclature.Utils;
using Nomenclature.Services;
using Nomenclature.Types;
using Serilog;
using System;
using Dalamud.Plugin.Services;
using NomenclatureCommon.Domain;
using NomenclatureCommon.Domain.Api;
using NomenclatureCommon.Domain.Api.Base;
using NomenclatureCommon.Domain.Api.Server;
using Microsoft.AspNetCore.SignalR.Client;

namespace Nomenclature.UI;

public class MainWindow : Window
{
    private readonly Configuration Configuration;
    private readonly WorldService WorldService;
    private readonly MainWindowController MainWindowController;
    private readonly NetworkService NetworkService;
    private readonly IPluginLog _log;
    private readonly RegistrationWindow RegistrationWindow;
    public MainWindow(IPluginLog log, Configuration configuration, WorldService worldService, MainWindowController mainWindowController, NetworkService networkService, RegistrationWindow registrationWindow) : base("Nomenclature")
    {
        Configuration = configuration;
        WorldService = worldService;
        MainWindowController = mainWindowController;
        NetworkService = networkService;
        _log = log;
        RegistrationWindow = registrationWindow;

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
                if(NetworkService.Connection.State == HubConnectionState.Connected)
                {
                    ImGui.NewLine();
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4() { W = 255, X = 0, Y = 255, Z = 0 });
                    SharedUserInterfaces.TextCentered("Connected!");
                    ImGui.PopStyleColor();
                    ImGui.SameLine();
                    if(SharedUserInterfaces.IconButton(FontAwesomeIcon.Unlink, tooltip: "Disconnect"))
                    {
                        NetworkService.Disconnect().ConfigureAwait(false);
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
                        NetworkService.Connect().ConfigureAwait(false);
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
            ImGui.Text("Name: ");
            ImGui.SameLine();

            try
            {
                if(ImGui.InputText("##NomenclatureName", ref MainWindowController.ChangedName, 32, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    _log.Verbose("Clicked me!");
                    UpdateName();
                }

                if (ImGui.Button("MIST"))
                {
                    _log.Verbose("CLICKED ME DAMNIT");
                    //UpdateName();
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
                    BlocklistCharacter blocklistChar = Configuration.BlocklistCharacters[i];

                    ImGui.PushID(blocklistChar.GUID.ToString());

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
                List<string> worldNames = WorldService.WorldNames;

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
                ImGui.InputTextWithHint("##BlocklistName", "Name", ref MainWindowController.BlocklistName, 32);

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
                ImGui.Combo("##BlocklistWorld", ref MainWindowController.BlocklistWorld, worldNames.ToArray(), worldNames.Count);
                
                ImGui.TableNextColumn();
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Plus))
                {
                    if (ValidateName(MainWindowController.BlocklistName))
                    {
                        Configuration.BlocklistCharacters.Add(new BlocklistCharacter { Name = MainWindowController.BlocklistName, World = worldNames[MainWindowController.BlocklistWorld] });
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

    private bool ValidateName(string name)
    {
        return name != null && name.Length > 0;
    }

    private async void UpdateName()
    {
        try
        {
            var request = new SetNameRequest
            {
                // TODO: Populate these fields with their real values!
                Nomenclature = new Character(string.Empty, string.Empty)
            };
            
            var response = await NetworkService.InvokeAsync<SetNameRequest, Response>(ApiMethods.SetName, request);
            if (response.Success is false)
                return;
            
            // TODO: Populate world too!
            Configuration.Name = MainWindowController.ChangedName;
            Configuration.Save();
        }
        catch(Exception ex)
        {
            _log.Debug(ex.ToString());
        }
    }
}