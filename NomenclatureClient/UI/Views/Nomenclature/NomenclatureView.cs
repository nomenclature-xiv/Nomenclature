using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using NomenclatureClient.Services;
using NomenclatureClient.UI.Windows;
using NomenclatureClient.Utils;

namespace NomenclatureClient.UI.Views.Nomenclature;

public class NomenclatureView(NomenclatureViewController controller, SettingsWindow settingsWindow, PairsWindow pairsWindow)
{
    public void Draw()
    {
        var padding = ImGui.GetStyle().WindowPadding;
        var size = ImGui.GetWindowSize();

        SharedUserInterfaces.ContentBox(() =>
        {
            SharedUserInterfaces.TextCentered("I am");
            ImGui.SameLine(size.X - padding.X * 7 - ImGui.GetFontSize() * 2);
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.PeopleGroup, tooltip: "Pairs"))
            {
                pairsWindow.Toggle();
            }
            ImGui.SameLine();
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Cog, tooltip: "Settings"))
            {
                settingsWindow.Toggle();
            }
            FontService.MediumFont?.Push();
            SharedUserInterfaces.TextCentered(controller.DisplayCurrentNomenclature());
            FontService.MediumFont?.Pop();
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            ImGui.BeginGroup();
            SharedUserInterfaces.DisableIf(controller.NomenclatureNameBehaviorIndex is not 1, () =>
            {
                ImGui.SetNextItemWidth(195);
                ImGui.InputTextWithHint("##OverrideNameInput", "Name", ref controller.NomenclatureName, 32);
            });
            ImGui.SetNextItemWidth(195);
            if (ImGui.Combo("##OverrideNameBehavior", ref controller.NomenclatureNameBehaviorIndex,
                    controller.Behaviors))
            {
            }
            ImGui.EndGroup();

            ImGui.SameLine();

            ImGui.BeginGroup();
            ImGui.TextUnformatted("@");
            ImGui.EndGroup();

            ImGui.SameLine();

            ImGui.BeginGroup();
            SharedUserInterfaces.DisableIf(controller.NomenclatureWorldBehaviorIndex is not 1, () =>
            {
                ImGui.SetNextItemWidth(195);
                ImGui.InputTextWithHint("##OverrideWorldInput", "World", ref controller.NomenclatureWorld, 32);
            });
            ImGui.SetNextItemWidth(195);
            if (ImGui.Combo("##OverrideWorldCombo", ref controller.NomenclatureWorldBehaviorIndex,
                    controller.Behaviors))
            {
            }
            ImGui.EndGroup();
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            ImGui.Text(String.Concat(
                controller.DisplayCurrentNomenclature(),
                " → ",
                controller.DisplayPendingNomenclature()));
        });

        SharedUserInterfaces.ContentBox(() =>
        {
            if (SharedUserInterfaces.IconButton(Dalamud.Interface.FontAwesomeIcon.Undo, new Vector2(50, 50), "Reset to default.", FontService.MediumFont))
            {
                _ =controller.RemoveNomenclature().ConfigureAwait(false);
            }
            ImGui.SameLine();
            FontService.MediumFont?.Push();
            if (ImGui.Button("Change Name", new Vector2(360, 50)))
            {
                _ = controller.SubmitChanges().ConfigureAwait(false);
            }
            FontService.MediumFont?.Pop();
        }, false);
    }
}