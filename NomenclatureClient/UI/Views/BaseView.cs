
using Dalamud.Bindings.ImGui;

namespace NomenclatureClient.UI.Views;

public static class BaseView
{
    public static void DrawTitle()
    {
        ImGui.PushStyleColor(ImGuiCol.TitleBg, Colors.PrimaryTransparent.Vector);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, Colors.Primary.Vector);
        ImGui.PushStyleColor(ImGuiCol.TitleBgCollapsed, Colors.Primary.Vector);
    }
    
    public static void Close()
    {
        ImGui.PopStyleColor();
        ImGui.PopStyleColor();
        ImGui.PopStyleColor();
    }
}