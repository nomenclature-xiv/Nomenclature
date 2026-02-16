using Dalamud.Bindings.ImGui;
using NomenclatureClient.Services;

namespace NomenclatureClient.UI.Views.Nomenclature;

public class NomenclatureView(NomenclatureViewController controller)
{
    public void Draw()
    {
        FontService.BigFont?.Push();
        ImGui.TextUnformatted("You are appearing as");
        FontService.BigFont?.Pop();

        ImGui.TextUnformatted(controller.DisplayCurrentNomenclature());
        
        ImGui.Separator();
        
        FontService.MediumFont?.Push();
        ImGui.TextUnformatted("Name");
        FontService.MediumFont?.Pop();
        
        ImGui.InputText("Name", ref controller.NomenclatureName);
        ImGui.Combo("Name Behavior", ref controller.NomenclatureNameBehaviorIndex, controller.Behaviors);
        
        FontService.MediumFont?.Push();
        ImGui.TextUnformatted("World");
        FontService.MediumFont?.Pop();
        ImGui.InputText("World", ref controller.NomenclatureWorld);
        ImGui.Combo("World Behavior", ref controller.NomenclatureWorldBehaviorIndex, controller.Behaviors);

        ImGui.Separator();
        
        FontService.BigFont?.Push();
        ImGui.TextUnformatted("You will appear as");
        FontService.BigFont?.Pop();

        ImGui.TextUnformatted(controller.DisplayPendingNomenclature());

        ImGui.Separator();
        
        FontService.MediumFont?.Push();
        if (ImGui.Button("Submit Changes"))
            _ = controller.SubmitChanges().ConfigureAwait(false);
        
        ImGui.SameLine();
        if (ImGui.Button("Remove Nomenclature"))
            _ =controller.RemoveNomenclature().ConfigureAwait(false);
        
        FontService.MediumFont?.Pop();
    }
}