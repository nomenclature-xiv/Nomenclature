using Dalamud.Bindings.ImGui;
using NomenclatureClient.Utils;
using System;
using System.Linq;

namespace NomenclatureClient.UI.Views.Pairs;

public class PairsView(PairsViewController controller)
{
    public void Draw()
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
        ImGui.TextUnformatted("Offline Pairs");
        SharedUserInterfaces.ContentBox(() =>
        {
            foreach (var item in controller.OfflinePairs.Keys)
            {
                ImGui.TextUnformatted(item);
                ImGui.SameLine();
                SharedUserInterfaces.IconButton(Dalamud.Interface.FontAwesomeIcon.Trash);
            }
        });
    }
}