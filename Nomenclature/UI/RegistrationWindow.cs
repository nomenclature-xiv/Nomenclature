using Dalamud.Interface.Windowing;
using ImGuiNET;
using Nomenclature.Services;
using Nomenclature.Utils;
using Dalamud.Interface;
using System.Numerics;
using System;
using Dalamud.Plugin.Services;
using NomenclatureCommon.Api;

namespace Nomenclature.UI
{
    public class RegistrationWindow : Window
    {
        private readonly NetworkService _networkService;
        private string _registrationKey = string.Empty;
        private bool registrationError = false;
        private bool characterError = false;
        private readonly IPluginLog _log;
        public RegistrationWindow(NetworkService networkService, IPluginLog log) : base("Register Character")
        {
            _networkService = networkService;
            _log = log;

            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(300, 300),
                MaximumSize = ImGui.GetIO().DisplaySize
            };
        }

        public override void Draw()
        {
            if (_registrationKey == string.Empty)
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
                ImGui.InputText("##RegistrationKey", ref _registrationKey, 64);
                ImGui.SameLine();
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Clipboard, tooltip: "Copy to Clipboard"))
                {
                    ImGui.SetClipboardText(_registrationKey);
                }
                if (ImGui.Button("Register"))
                {
                    Register();
                }
                if (registrationError)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4() { W = 255, X = 255, Y = 0, Z = 0 });
                    ImGui.TextWrapped("Could not find code in character's profile! Ensure that it is saved before trying again.");
                    ImGui.PopStyleColor();
                }
            }
            
        }

        public override void OnOpen()
        {
            Register();
            base.OnOpen();
        }
        
        private async void Register()
        {
            try
            {
                var result = await _networkService.RegisterCharacterInitiate("Mora Nightshade", "Diabolos");
                _log.Info($"Result: {result}");
                if(result is not null)
                {
                    _registrationKey = result;
                    characterError = false;
                }
                else
                {
                    characterError = true;
                }
            }
            catch (Exception e)
            {
                _log.Error($"{e}");
            }
        }
    }
}
