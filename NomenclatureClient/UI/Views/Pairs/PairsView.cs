using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using NomenclatureClient.Services;
using NomenclatureClient.Utils;

namespace NomenclatureClient.UI.Views.Pairs;

public class PairsView(PairsViewController controller)
{
    
    public void Draw()
    {
        var padding = ImGui.GetStyle().WindowPadding;
        var size = ImGui.GetWindowSize();
        SharedUserInterfaces.ContentBox(() =>
        {
            FontService.BigFont?.Push();
            SharedUserInterfaces.InvisSelectableCentered(controller.SyncCode);
            FontService.BigFont?.Pop();
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                ImGui.SetTooltip("Click to copy");
            }
            if(ImGui.IsItemClicked())
            {
                ImGui.SetClipboardText(controller.SyncCode);
            }
        });
        SharedUserInterfaces.ContentBox(() =>
        {
            ImGui.SetNextItemWidth(size.X - padding.X - ImGui.GetFontSize() * 3);
            ImGui.InputTextWithHint("##Friend", "Other person's UID", ref controller.guh);
            ImGui.SameLine();
            if (SharedUserInterfaces.IconButton(Dalamud.Interface.FontAwesomeIcon.Plus, tooltip: "Add Friend"))
                controller.Add();
        });
        ImGui.BeginChild("###ScrollingRegion", flags: ImGuiWindowFlags.AlwaysVerticalScrollbar);
        if (controller.OnlinePairs.Count > 0)
        {
            DrawAllPairs("Online Pairs", controller.OnlinePairs.Keys, ImGuiTreeNodeFlags.DefaultOpen);
        }
        if (controller.PendingPairs.Count > 0)
        {
            DrawAllPairs("Pending Pairs", controller.PendingPairs.Keys);
        }
        if (controller.OfflinePairs.Count > 0)
        {
            DrawAllPairs("Offline Pairs", controller.OfflinePairs.Keys);
        }
        else
        {
            ImGui.TextUnformatted("You have no pairs! Try adding one.");
        }

        ImGui.EndChild();
    }

    private void DrawAllPairs(string title, IEnumerable<string> keys, ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.None)
    {
        var padding = ImGui.GetStyle().WindowPadding;
        var size = ImGui.GetWindowSize();
        if (ImGui.CollapsingHeader(title, flags))
        {
            SharedUserInterfaces.ContentBox(() =>
            {
                foreach (var item in keys)
                {
                    controller.Nicknames.TryGetValue(item, out var nickname);
                    ImGui.AlignTextToFramePadding();
                    if (ImGui.Selectable(controller.Editing == item ? $"##{item}" : nickname ?? item,
                            size: new Vector2(controller.Editing == item ? 0.001f : 0f, 0f),
                            flags: ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap))
                    {
                    }

                    if (controller.Editing == item)
                    {
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(size.X - padding.X - ImGui.GetFontSize() * 4.5f);
                        if (!controller.FocusSet)
                        {
                            ImGui.SetKeyboardFocusHere();
                            controller.FocusSet = true;
                        }
                        if (ImGui.InputTextWithHint($"##EditingInput{item}", "Nickname", ref controller.Nickname,
                                flags: ImGuiInputTextFlags.EnterReturnsTrue))
                        {
                            controller.HandleNickname(item);
                        }
                    }
                    if (ImGui.IsItemHovered())
                    {
                        if (controller.Editing == item)
                            ImGui.SetTooltip("Enter to save\nRight click to revert");
                        else
                            ImGui.SetTooltip("Right click to modify nickname");
                        if (ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                        {
                            controller.Nicknames.TryGetValue(item, out nickname);
                            controller.Nickname = nickname ?? "";
                            controller.Editing = controller.Editing == item ? "" : item;
                            controller.FocusSet = false;
                        }
                    }
                    ImGui.SameLine(size.X - padding.X - ImGui.GetFontSize() * 2.5f);
                    if (SharedUserInterfaces.IconButton(Dalamud.Interface.FontAwesomeIcon.Trash, label: item))
                        _ = controller.Remove(item).ConfigureAwait(false);
                }
            });
        }
    }
}