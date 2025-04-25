using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Interface;
using Dalamud.Interface.ManagedFontAtlas;
using ImGuiNET;

namespace Nomenclature.Utils;

/// <summary>
///     Exposes multiple static methods that simplify the process of many ImGui objects
/// </summary>
public static class SharedUserInterfaces
{
    private const ImGuiWindowFlags PopupWindowFlags =
        ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize;

    private const ImGuiWindowFlags ComboWithFilterFlags = PopupWindowFlags | ImGuiWindowFlags.ChildWindow;

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

    /// <summary>
    ///     Creates a button the size of a <see cref="ContentBox"/> on the right
    /// </summary>
    public static bool ContextBoxButton(FontAwesomeIcon icon, Vector2 padding, float windowWidth)
    {
        var previousRectSize = ImGui.GetItemRectSize();
        var returnPoint = ImGui.GetCursorPosY();
        var begin = returnPoint - previousRectSize.Y - padding.Y * 2;

        var x = windowWidth - previousRectSize.Y - padding.X;
        var size = new Vector2(x, begin);

        ImGui.SetCursorPos(size);
        var clicked = IconButton(icon, new Vector2(previousRectSize.Y));
        ImGui.SetCursorPosY(returnPoint);
        return clicked;
    }
}