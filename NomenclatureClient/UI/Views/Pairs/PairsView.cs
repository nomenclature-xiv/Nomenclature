using Dalamud.Bindings.ImGui;
using NomenclatureClient.Utils;

namespace NomenclatureClient.UI.Views.Pairs;

public class PairsView(PairsViewController controller)
{
    // TODO: This doesn't actually display the individual categories
    
    public void Draw()
    {
        ImGui.InputText("##Friend", ref controller.guh);
        ImGui.SameLine();
        if (SharedUserInterfaces.IconButton(Dalamud.Interface.FontAwesomeIcon.Plus, tooltip: "Add Friend"))
            controller.Add();
        if (controller.OnlinePairs.Count > 0)
        {
            ImGui.TextUnformatted("Online Pairs");
            SharedUserInterfaces.ContentBox(() =>
            {
                foreach (var item in controller.OnlinePairs.Keys)
                {
                    ImGui.TextUnformatted(item);
                    ImGui.SameLine();
                    if (SharedUserInterfaces.IconButton(Dalamud.Interface.FontAwesomeIcon.Pause))
                        _ = controller.Pause(item).ConfigureAwait(false);
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