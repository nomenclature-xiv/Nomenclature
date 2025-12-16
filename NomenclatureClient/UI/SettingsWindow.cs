using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using NomenclatureClient.Network;
using NomenclatureClient.Utils;
using System.Numerics;

namespace NomenclatureClient.UI
{
    public class SettingsWindow : Window
    {
        private NetworkService _networkService;
        public SettingsWindow(NetworkService networkService) : base("Settings", ImGuiWindowFlags.NoResize)
        {
            _networkService = networkService;
            SizeConstraints = new WindowSizeConstraints
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
            });
        }
    }
}
