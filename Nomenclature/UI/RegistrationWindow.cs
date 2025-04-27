using Dalamud.Interface.Windowing;
using ImGuiNET;
using Nomenclature.Services;
using Nomenclature.Utils;
using Dalamud.Interface;
using System.Numerics;
using System;
using Dalamud.Plugin.Services;
using System.Collections.Generic;
using Nomenclature.Network;

namespace Nomenclature.UI
{
    public class RegistrationWindow : Window
    {
        private readonly RegistrationWindowController _controller;
        private readonly IPluginLog _log;
        public RegistrationWindow(RegistrationWindowController controller, IPluginLog log) : base("Register Character")
        {
            _controller = controller;
            _log = log;

            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(300, 300),
                MaximumSize = ImGui.GetIO().DisplaySize
            };
        }

        public override void Draw()
        {
            if (_controller.RegistrationKey == string.Empty)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4() { W = 255, X = 0, Y = 255, Z = 0 });
                ImGui.Text("Fetching character data from lodestone...");
                ImGui.PopStyleColor();
            }
            else
            {
                ImGui.TextWrapped("Post the following code to the current character's lodestone profile, then click the 'Register' button. You may delete it when you are registered with Nomenclature.");
                ImGui.NewLine();

                ImGui.SetNextItemWidth(200);
                ImGui.InputText("##RegistrationKey", ref _controller.RegistrationKey, 64);
                ImGui.SameLine();
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Clipboard, tooltip: "Copy to Clipboard"))
                {
                    ImGui.SetClipboardText(_controller.RegistrationKey);
                }
                if (ImGui.Button("Register"))
                {
                    Validate();
                }
                if (_controller.CharacterError)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4() { W = 255, X = 255, Y = 0, Z = 0 });
                    ImGui.TextWrapped("Your character somehow doesn't exist! You shouldn't be seeing this, but if you do, try clicking the 'Regenerate Secret' button.");
                    ImGui.PopStyleColor();
                    if(ImGui.Button("Regenerate Secret"))
                    {
                        _controller.InitRegister();
                    }
                }
                if (_controller.RegistrationError)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4() { W = 255, X = 255, Y = 0, Z = 0 });
                    ImGui.TextWrapped("Could not find code in character's profile! Ensure that it is saved before trying again.");
                    ImGui.PopStyleColor();
                }
            }
            
        }

        private async void Validate()
        {
            bool res = await _controller.ValidateRegister();
            if(res)
            {
                this.Toggle();
            }
        }

        public override void OnOpen()
        {
            _controller.InitRegister();
            base.OnOpen();
        }
    }
}
