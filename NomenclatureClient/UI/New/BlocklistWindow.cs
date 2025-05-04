using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using NomenclatureClient.Services;
using NomenclatureClient.Utils;
using NomenclatureCommon.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NomenclatureClient.UI.New
{
    public class BlocklistWindow : Window
    {
        private readonly Configuration _config;
        private readonly WorldService _worldService;
        private readonly BlocklistWindowController _controller;
        private List<string> _worldNames;

        public BlocklistWindow(Configuration config, WorldService worldService, BlocklistWindowController controller) : base("Nomenclature Blocklist")
        {
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(400, 200),
                MaximumSize = ImGui.GetIO().DisplaySize
            };

            _config = config;
            _worldService = worldService;
            _controller = controller;
            _worldNames = _worldService.WorldNames;
        }

        public override void Draw()
        {
            SharedUserInterfaces.ContentBox(() =>
            {
                int cnt = 3;
                if (ImGui.BeginTable("##BlocklistTable", cnt, ImGuiTableFlags.Borders))
                {
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.None, 200);
                    ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.None, 120);
                    ImGui.TableSetupColumn(" ", ImGuiTableColumnFlags.NoResize, 20);
                    ImGui.TableHeadersRow();

                    for (int i = 0; i < _config.BlocklistCharacters.Count; i++)
                    {
                        Character blocklistChar = _config.BlocklistCharacters[i];

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
                            _config.BlocklistCharacters.Remove(blocklistChar);
                            _config.Save();
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
                        if (ValidateName(_controller.BlocklistName))
                        {
                            _config.BlocklistCharacters.Add(new Character(_controller.BlocklistName, _worldNames[_controller.BlocklistWorld]));
                            _config.Save();
                            _controller.BlocklistName = string.Empty;
                            _controller.BlocklistWorld = 0;
                        }
                    }
                    ImGui.EndTable();
                }
            });
        }
        private bool ValidateName(string name)
        {
            return name != null && name.Length > 0;
        }
    }
}
