using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using NomenclatureClient.Network;
using NomenclatureClient.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonActionBarX;

namespace NomenclatureClient.UI
{
    public class SettingsWindow : Window
    {
        private NetworkService _networkService;
        private BlocklistWindow _blocklistWindow;
        public SettingsWindow(NetworkService networkService, BlocklistWindow blocklistWindow) : base("Settings", ImGuiWindowFlags.NoResize)
        {
            _networkService = networkService;
            _blocklistWindow = blocklistWindow;
            SizeConstraints = new()
            {
                MinimumSize = new Vector2(250, 110),
                MaximumSize = new Vector2(250, 110)
            };
        }
        public override void Draw()
        {
            SharedUserInterfaces.ContentBox(async () =>
            {
                var dimensions = new Vector2((ImGui.GetWindowSize().X - ImGui.GetStyle().WindowPadding.X * 4), 0);
                if (ImGui.Button("Disconnect", dimensions))
                {
                    _networkService.Disconnect();
                }
                if (ImGui.Button("Blocklist", dimensions))
                {
                    _blocklistWindow.Toggle();

                }
            });
        }
    }
}
