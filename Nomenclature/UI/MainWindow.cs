using System.Collections.Generic;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Nomenclature.Utils;
using Nomenclature.Services;
using Nomenclature.Types;
using Serilog;
using System;
using NomenclatureCommon.Domain.Api;
using NomenclatureCommon;

namespace Nomenclature.UI;

public class MainWindow : Window
{
    private readonly Configuration Configuration;
    private readonly WorldService WorldService;
    private readonly MainWindowController MainWindowController;
    private readonly NetworkService NetworkService;
    public MainWindow(Configuration configuration, WorldService worldService, MainWindowController mainWindowController, NetworkService networkService) : base("Nomenclature")
    {
        Configuration = configuration;
        WorldService = worldService;
        MainWindowController = mainWindowController;
        NetworkService = networkService;

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
            DrawMainTab();
            DrawBlocklistTab();
            DrawSettingsTab();
            ImGui.EndTabBar();
        }
    }

    private void DrawMainTab()
    {
        if(ImGui.BeginTabItem("Nomenclature"))
        {
            ImGui.Text("Name: ");
            ImGui.SameLine();
            if(ImGui.InputText("##NomenclatureName", ref MainWindowController.ChangedName, 32, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                UpdateName();
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

    private void DrawSettingsTab()
    {
        if (ImGui.BeginTabItem("Settings"))
        {
            ImGui.Text("Test");
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
            RegisterNameResponse response = await NetworkService.InvokeAsync<RegisterNameRequest, RegisterNameResponse>(ApiMethod.RegisterName, new RegisterNameRequest { Name = MainWindowController.ChangedName });
            if(response.Success)
            {
                Configuration.Name = MainWindowController.ChangedName;
                Configuration.Save();
                Log.Verbose("Successfully changed name!");
            }
        }
        catch(Exception ex)
        {
            Log.Debug(ex.ToString());
        }
    }
}