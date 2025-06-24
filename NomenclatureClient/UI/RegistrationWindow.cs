using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using NomenclatureClient.Utils;

namespace NomenclatureClient.UI;

public class RegistrationWindow : Window
{
    private readonly RegistrationWindowController _controller;
    
    public RegistrationWindow(RegistrationWindowController controller) : base("Nomenclature - Registration")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 280),
            MaximumSize = ImGui.GetIO().DisplaySize
        };
        
        _controller = controller;
    }

    public override void Draw()
    {
        if (ImGui.BeginChild("##NomenclatureRegistrationWindow", Vector2.Zero, false) is false)
            return;
        
        if (_controller.RegistrationKey is null)
        {
            SharedUserInterfaces.ContentBox(() =>
            {
                ImGui.TextColored(ImGuiColors.ParsedGreen, "Retrieving current character data from lodestone...");
            });
            
            ImGui.EndChild();
            return;
        }

        SharedUserInterfaces.ContentBox(() =>
        {
            ImGui.TextWrapped("Post the following code to the current character's lodestone profile, then click the 'Validate' button. You may delete it when you are registered with Nomenclature.");

            if (_controller.BeginRegistrationError)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
                ImGui.TextWrapped("Your character somehow doesn't exist! You shouldn't be seeing this, but if you do, try clicking the 'Regenerate Secret' button.");
                ImGui.PopStyleColor();
                
                if (ImGui.Button("Regenerate Secret"))
                    _controller.BeginRegistration();
            }

            if (_controller.ValidateRegistrationError)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
                ImGui.TextWrapped("Could not find code in character's profile! Ensure that it is saved before trying again.");
                ImGui.PopStyleColor();
            }
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - ImGui.GetStyle().WindowPadding.X * 6);
            ImGui.InputText("##RegistrationKey", ref _controller.RegistrationKey, 64);
            ImGui.SameLine();
            
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Clipboard, null, "Copy to Clipboard"))
                ImGui.SetClipboardText(_controller.RegistrationKey);
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            if (ImGui.Button("Validate", new Vector2(ImGui.GetWindowWidth() - ImGui.GetStyle().WindowPadding.X * 2, 0)))
                ValidateRegistration();
        });
        
        ImGui.EndChild();
    }

    private async void ValidateRegistration()
    {
        try
        {
            if (await _controller.ValidateRegistration())
                Toggle();
        }
        catch (Exception )
        {
            // Ignore
        }
    }

    public override void OnOpen()
    {
        _controller.BeginRegistration();
        base.OnOpen();
    }
}