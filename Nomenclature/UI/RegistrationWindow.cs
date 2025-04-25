using Dalamud.Interface.Windowing;
using ImGuiNET;
using Nomenclature.Services;
using Nomenclature.Utils;
using Dalamud.Interface;
using System.Numerics;
using System;

namespace Nomenclature.UI
{
    public class RegistrationWindow : Window
    {
        private readonly NetworkService _networkService;
        private string _registrationKey = string.Empty;
        private bool registrationError = false;
        public RegistrationWindow(NetworkService networkService) : base("Register Character")
        {
            _networkService = networkService;

            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(300, 300),
                MaximumSize = ImGui.GetIO().DisplaySize
            };
        }

        public override void Draw()
        {
            ImGui.TextWrapped("Post the following code to the current character's lodestone profile, then click the 'Register' button. You may delete it when you are registered with Nomenclature.");
            ImGui.NewLine();

            ImGui.SetNextItemWidth(200);
            ImGui.InputText("##RegistrationKey", ref _registrationKey, 64);
            ImGui.SameLine();
            if(SharedUserInterfaces.IconButton(FontAwesomeIcon.Clipboard, tooltip: "Copy to Clipboard"))
            {
                ImGui.SetClipboardText(_registrationKey);
            }
            if(ImGui.Button("Register"))
            {

            }
            if(registrationError)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4() { W = 255, X = 255, Y = 0, Z = 0 });
                ImGui.TextWrapped("Could not find code in character's profile! Ensure that it is saved before trying again.");
                ImGui.PopStyleColor();
            }
            
        }

        public override void OnOpen()
        {
            base.OnOpen();
        }
    }
}
