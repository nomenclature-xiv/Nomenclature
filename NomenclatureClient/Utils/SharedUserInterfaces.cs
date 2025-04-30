using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Interface;
using Dalamud.Interface.ManagedFontAtlas;
using ImGuiNET;
using NomenclatureClient.Services;

namespace NomenclatureClient.Utils;

/// <summary>
///     Exposes multiple static methods that simplify the process of many ImGui objects
/// </summary>
public static class SharedUserInterfaces
{
    private const ImGuiWindowFlags PopupWindowFlags = 
        ImGuiWindowFlags.NoTitleBar | 
        ImGuiWindowFlags.NoMove | 
        ImGuiWindowFlags.NoResize;
    
    private static readonly uint PanelBackground = ImGui.ColorConvertFloat4ToU32(new Vector4(0.1294f, 0.1333f, 0.1764f, 1));
    
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
    ///     Draws a tool tip for the last hovered ImGui component
    /// </summary>
    /// <param name="tip"></param>
    public static void Tooltip(string tip)
    {
        if (ImGui.IsItemHovered() is false)
            return;

        ImGui.BeginTooltip();
        ImGui.Text(tip);
        ImGui.EndTooltip();
    }

    /// <summary>
    ///     Draws a tool tip for the last hovered ImGui component
    /// </summary>
    /// <param name="tips"></param>
    public static void Tooltip(string[] tips)
    {
        if (ImGui.IsItemHovered() is false)
            return;

        ImGui.BeginTooltip();
        foreach (var tip in tips)
            ImGui.Text(tip);

        ImGui.EndTooltip();
    }

    /// <summary>
    ///     Draws a <see cref="FontAwesomeIcon"/>
    /// </summary>
    public static void Icon(FontAwesomeIcon icon, Vector4? color = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        if (color is null)
            ImGui.TextUnformatted(icon.ToIconString());
        else
            ImGui.TextColored(color.Value, icon.ToIconString());
        ImGui.PopFont();
    }

    /// <summary>
    ///     Creates a button with specified icon
    /// </summary>
    public static bool IconButton(FontAwesomeIcon icon, Vector2? size = null, string? tooltip = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var result = ImGui.Button(icon.ToIconString(), size ?? Vector2.Zero);
        ImGui.PopFont();

        if (tooltip is null)
            return result;

        if (ImGui.IsItemHovered() is false)
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

    public static bool ButtonCentered(string text)
    {
        ImGui.SetCursorPosX((ImGui.GetWindowWidth() - ImGui.CalcTextSize(text).X) * 0.5f);
        return ImGui.Button(text);
    }
    
}