using Dalamud.Bindings.ImGui;
using NomenclatureClient.Utils;
using System;
using System.Linq;

namespace NomenclatureClient.UI.Views.Pairs;

public class PairsView(PairsViewController controller)
{
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
                        controller.Pause(item);
                    ImGui.SameLine();
                    if (SharedUserInterfaces.IconButton(Dalamud.Interface.FontAwesomeIcon.Trash))
                        controller.Remove(item);
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
                        controller.Remove(item);
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
                        controller.Remove(item);
                }
            });
        }
        else
        {
            ImGui.TextUnformatted("You have no pairs! Try adding one.");
        }
    }
}