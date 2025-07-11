using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using NomenclatureClient.Services;
using NomenclatureClient.Utils;

namespace NomenclatureClient.UI;

public class BlocklistWindow : Window
{
    private readonly SessionService _sessionService;
    
    private readonly BlocklistWindowController _controller;
    private readonly List<string> _worldNames;

    public BlocklistWindow(
        SessionService sessionService,
        BlocklistWindowController controller,
        WorldService worldService) : base("Nomenclature Blocklist")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 200),
            MaximumSize = ImGui.GetIO().DisplaySize
        };
        
        _sessionService = sessionService;
        _controller = controller;
        _worldNames = worldService.WorldNames;
    }

    public override void Draw()
    {
        var blockList = _sessionService.CurrentSession.CharacterConfiguration.BlockedCharacters;
        SharedUserInterfaces.ContentBox(() =>
        {
            if (ImGui.BeginTable("##BlocklistTable", 3, ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.None, 200);
                ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.None, 120);
                ImGui.TableSetupColumn(" ", ImGuiTableColumnFlags.NoResize, 20);
                ImGui.TableHeadersRow();

                int counter = 0;
                foreach (var blocked in blockList.ToList())
                {
                    ImGui.PushID(counter++);
                    var split = blocked.Split('@');
                    
                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
                    ImGui.Text(split[0]);

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
                    ImGui.Text(split[1]);
                    
                    ImGui.TableNextColumn();
                    if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Trash, null, "Delete"))
                    {
                        blockList.Remove(blocked);
                        _sessionService.Save();
                    }
                    ImGui.PopID();
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
                ImGui.InputTextWithHint("##BlocklistName", "Name", ref _controller.BlocklistName, 32);

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
                ImGui.Combo("##BlocklistWorld", ref _controller.BlocklistWorld, _worldNames.ToArray(), _worldNames.Count);

                ImGui.TableNextColumn();
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Plus))
                {
                    if (_controller.BlocklistName.Length > 0)
                    {
                        blockList.Add(string.Concat(_controller.BlocklistName, "@", _worldNames[_controller.BlocklistWorld]));
                        _sessionService.Save();
                        _controller.BlocklistName = string.Empty;
                        _controller.BlocklistWorld = 0;
                    }
                }
                ImGui.EndTable();
            }
        });
    }
}