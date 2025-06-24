using System;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;

namespace NomenclatureClient.Utils;

/// <summary>
///     Exposes multiple static methods that simplify the process of many ImGui objects
/// </summary>
public static class SharedUserInterfaces
{
    // Const
    private static readonly uint PanelBackground = ImGui.ColorConvertFloat4ToU32(new Vector4(0.1294f, 0.1333f, 0.1764f, 1));
    
    /// <summary>
    ///     Display elements inside a rounded box, stretching to the window width
    /// </summary>
    public static void ContentBox(Action contentToDraw, bool addSpacingAtEnd = true)
    {
        var windowPadding = ImGui.GetStyle().WindowPadding;
        var drawList = ImGui.GetWindowDrawList();
        drawList.ChannelsSplit(2);
        drawList.ChannelsSetCurrent(1);

        var startPosition = ImGui.GetCursorPos();
        var anchorPoint = ImGui.GetCursorScreenPos();
        ImGui.SetCursorPos(startPosition + windowPadding);

        ImGui.BeginGroup();
        contentToDraw.Invoke();
        ImGui.EndGroup();

        drawList.ChannelsSetCurrent(0);

        var min = ImGui.GetItemRectMin() - windowPadding;
        var max = ImGui.GetItemRectMax() + windowPadding;
        max.X = anchorPoint.X + ImGui.GetWindowWidth();

        ImGui.GetWindowDrawList().AddRectFilled(min, max, PanelBackground, 4);
        drawList.ChannelsMerge();
        ImGui.SetCursorPosY(startPosition.Y + (max.Y - min.Y) + (addSpacingAtEnd ? windowPadding.Y : 0));
    }

    /// <summary>
    ///     Disables the content draw if the provided condition is true
    /// </summary>
    public static void DisableIf(bool condition, Action contentToDraw)
    {
        if (condition)
            ImGui.BeginDisabled();
        
        contentToDraw.Invoke();
        
        if (condition)
            ImGui.EndDisabled();
    }

    /// <summary>
    ///     Creates a button with specified icon
    /// </summary>
    public static bool IconButton(FontAwesomeIcon icon, Vector2? size = null, string? tooltip = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var result = ImGui.Button(icon.ToIconString(), size ?? Vector2.Zero);
        ImGui.PopFont();

        if (tooltip is null || ImGui.IsItemHovered() is false)
            return result;

        ImGui.BeginTooltip();
        ImGui.TextUnformatted(tooltip);
        ImGui.EndTooltip();

        return result;
    }

    /// <summary>
    ///     Draws text using the default font, centered, with optional color.
    /// </summary>
    public static void TextCentered(string text, Vector4? color = null)
    {
        ImGui.SetCursorPosX((ImGui.GetWindowWidth() - ImGui.CalcTextSize(text).X) * 0.5f);
        if (color is null)
            ImGui.TextUnformatted(text);
        else
            ImGui.TextColored(color.Value, text);
    }
}