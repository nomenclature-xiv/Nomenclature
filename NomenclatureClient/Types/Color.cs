using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace NomenclatureClient.Types;

public class Color
{
    public Vector4 Vector;
    public uint Uint;
    public Color(uint abgr)
    {
        Uint = abgr;
        Vector = ImGui.ColorConvertU32ToFloat4(abgr);
    }
        
}