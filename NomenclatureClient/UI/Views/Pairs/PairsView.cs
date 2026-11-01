using Dalamud.Bindings.ImGui;
using NomenclatureClient.Services;
using NomenclatureClient.Utils;

namespace NomenclatureClient.UI.Views.Pairs;

public class PairsView(PairsViewController controller)
{
    
    public void Draw()
    {
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
            var padding = ImGui.GetStyle().WindowPadding;
            var size = ImGui.GetWindowSize();
            ImGui.SetNextItemWidth(size.X - padding.X - ImGui.GetFontSize() * 3);
            ImGui.InputTextWithHint("##Friend", "Other person's UID", ref controller.guh);
            ImGui.SameLine();
            if (SharedUserInterfaces.IconButton(Dalamud.Interface.FontAwesomeIcon.Plus, tooltip: "Add Friend"))
                controller.Add();
        });
        if (controller.OnlinePairs.Count > 0)
        {
            ImGui.TextUnformatted("Online Pairs");
            SharedUserInterfaces.ContentBox(() =>
            {
                foreach (var item in controller.OnlinePairs.Keys)
                {
                    ImGui.TextUnformatted(item);
                    ImGui.SameLine();
                    if (SharedUserInterfaces.IconButton(Dalamud.Interface.FontAwesomeIcon.Trash))
                        _ = controller.Remove(item).ConfigureAwait(false);
                }
            });
        }
        else if (controller.PendingPairs.Count > 0)
        {
            ImGui.TextUnformatted("Pending Pairs");
            SharedUserInterfaces.ContentBox(() =>
            {
                foreach (var item in controller.PendingPairs.Keys)
                {
                    ImGui.TextUnformatted(item);
                    ImGui.SameLine();
                    if (SharedUserInterfaces.IconButton(Dalamud.Interface.FontAwesomeIcon.Trash))
                        _ = controller.Remove(item).ConfigureAwait(false);
                }
            });
        }
        else if (controller.OfflinePairs.Count > 0)
        {
            ImGui.TextUnformatted("Offline Pairs");
            SharedUserInterfaces.ContentBox(() =>
            {
                foreach (var item in controller.OfflinePairs.Keys)
                {
                    ImGui.TextUnformatted(item);
                    ImGui.SameLine();
                    if (SharedUserInterfaces.IconButton(Dalamud.Interface.FontAwesomeIcon.Trash))
                        _ = controller.Remove(item).ConfigureAwait(false);
                }
            });
        }
        else
        {
            ImGui.TextUnformatted("You have no pairs! Try adding one.");
        }
    }
}