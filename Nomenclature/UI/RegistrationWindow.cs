using Dalamud.Interface.Windowing;
using ImGuiNET;
using Nomenclature.Services;
using Nomenclature.Utils;
using Dalamud.Interface;
using System.Numerics;
using System;
using Dalamud.Plugin.Services;
using System.Collections.Generic;

namespace Nomenclature.UI
{
    public class RegistrationWindow : Window
    {
        private readonly NetworkService _networkService;
        private readonly CharacterService _characterService;
        private readonly Configuration _configuration;
        private readonly IClientState _clientState;
        private string _registrationKey = string.Empty;
        private bool registrationError = false;
        private bool characterError = false;
        private readonly IPluginLog _log;
        public RegistrationWindow(NetworkService networkService, CharacterService characterService, Configuration configuration, IPluginLog log, IClientState clientState) : base("Register Character")
        {
            _networkService = networkService;
            _characterService = characterService;
            _clientState = clientState;
            _configuration = configuration;
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
                    ValidateRegister();
                }
                if (characterError)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4() { W = 255, X = 255, Y = 0, Z = 0 });
                    ImGui.TextWrapped("Your character somehow doesn't exist! You shouldn't be seeing this, but if you do, try clicking the 'Regenerate Secret' button.");
                    ImGui.PopStyleColor();
                    if(ImGui.Button("Regenerate Secret"))
                    {
                        InitRegister();
                    }
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
            InitRegister();
            base.OnOpen();
        }
        
        private async void InitRegister()
        {
            try
            {
                var name = _characterService.CurrentCharacter;
                if (name is null) return;
                var result = await _networkService.RegisterCharacterInitiate(name);
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

        private async void ValidateRegister()
        {
            try
            {
                var name = _characterService.CurrentCharacter;
                if (name is null) return;
                var result = await _networkService.RegisterCharacterValidate(_registrationKey);
                if(result is not null)
                {
                    registrationError = false;
                    _configuration.LocalCharacters.TryGetValue(name.Name, out Dictionary<string, string>? worldsecret);
                    if(worldsecret is null)
                    {
                        worldsecret = new Dictionary<string, string>();
                    }
                    worldsecret.Add(name.World, result);
                    _configuration.LocalCharacters[name.Name] = worldsecret;
                    _configuration.Save();
                    await _networkService.Connect().ConfigureAwait(false);
                    this.Toggle();
                }
                else
                {
                    registrationError = true;
                }
            }
            catch(Exception e)
            {
                _log.Error($"{e}");
            }
        }
    }
}
