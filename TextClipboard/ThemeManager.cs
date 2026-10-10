using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Forms = System.Windows.Forms;
using DrawingColor = System.Drawing.Color;
using LinearGradientBrush = System.Windows.Media.LinearGradientBrush;

namespace TextClipboard
{
    public sealed record ThemePalette(
        string Name,
        string Window, string Panel, string Surface, string Header,
        string HeaderLight, string Border, string Text, string Muted,
        string Accent, string SelectedText, string Button,
        string ButtonLight, string Hover, string Popup);

    public static class ThemeManager
    {
        public static readonly IReadOnlyList<ThemePalette> Themes = new[]
        {
            new ThemePalette("Olive Classic", "#ECEBD8", "#F5F5E8", "#FFFEF6", "#697941", "#A5AE74",
                "#A1A780", "#27301C", "#65715A", "#7C9150", "#FFFFFF", "#D8DEBF", "#F7F8E8", "#CAD6A1", "#F1F0D7"),
            new ThemePalette("Luna Blue", "#DAE8FB", "#EDF5FE", "#FFFFFF", "#1555C6", "#6AA1F3",
                "#7AA7DE", "#152C53", "#597493", "#3977D3", "#FFFFFF", "#C7DAF6", "#F5FAFF", "#B9D6FF", "#E7F2FF"),
            new ThemePalette("Silver Frost", "#E2E6EF", "#F2F4F8", "#FFFFFF", "#6D7D99", "#BAC7DB",
                "#A8B5C9", "#24334D", "#65738B", "#6E91C5", "#FFFFFF", "#D5DDE9", "#F9FBFF", "#CBD9ED", "#F0F4FA"),
            new ThemePalette("Classic Gray", "#D4D0C8", "#E9E7E1", "#FFFFFF", "#0A246A", "#36599B",
                "#848484", "#202020", "#575757", "#0A246A", "#FFFFFF", "#D4D0C8", "#F2F0EB", "#C7D3E9", "#E7E5DF"),
            new ThemePalette("Night Mode", "#171B24", "#252B38", "#1C2230", "#273652", "#475C83",
                "#56647D", "#F1F3F8", "#BCC6D5", "#547EBA", "#FFFFFF", "#354258", "#53627A", "#3D5273", "#2A3444")
        };

        public static ThemePalette Current { get; private set; } = Themes[0];

        public static void ApplyTheme(string? name)
        {
            Current = Themes.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase))
                ?? Themes[0];
            var palette = Current;
            var resources = System.Windows.Application.Current.Resources;
            Set("WindowBrush", palette.Window);
            Set("PanelBrush", palette.Panel);
            Set("SurfaceBrush", palette.Surface);
            Set("BorderBrush", palette.Border);
            Set("BodyTextBrush", palette.Text);
            Set("MutedTextBrush", palette.Muted);
            Set("AccentBrush", palette.Accent);
            Set("SelectedTextBrush", palette.SelectedText);
            Set("ButtonBrush", palette.Button);
            Set("ButtonLightBrush", palette.ButtonLight);
            Set("HoverBrush", palette.Hover);
            Set("PopupBrush", palette.Popup);
            resources["TitleGradientBrush"] = Gradient(palette.HeaderLight, palette.Header);
            resources["SectionGradientBrush"] = Gradient(palette.ButtonLight, palette.Button);
            resources[System.Windows.SystemColors.HighlightBrushKey] = ParseBrush(palette.Accent);
            resources[System.Windows.SystemColors.HighlightTextBrushKey] = ParseBrush(palette.SelectedText);
            resources[System.Windows.SystemColors.ControlBrushKey] = ParseBrush(palette.Panel);
            resources[System.Windows.SystemColors.ControlTextBrushKey] = ParseBrush(palette.Text);

            void Set(string key, string hex) => resources[key] = ParseBrush(hex);
        }

        public static Forms.ToolStripRenderer CreateMenuRenderer()
            => new Forms.ToolStripProfessionalRenderer(new RetroColorTable(Current));

        public static void StylePopup(Forms.ToolStripDropDown menu)
        {
            ThemePalette p = Current;
            menu.BackColor = DrawingColorTranslator(p.Popup);
            menu.ForeColor = DrawingColorTranslator(p.Text);
            menu.Font = new System.Drawing.Font("Tahoma", 9.0f);
            menu.Renderer = CreateMenuRenderer();
            menu.AllowTransparency = true;
            menu.Opacity = 0.96;
            menu.Padding = new Forms.Padding(5);
            menu.ShowItemToolTips = true;
            menu.SizeChanged += (_, _) => RoundMenu(menu);
            menu.Opened += (_, _) => RoundMenu(menu);
        }

        public static void StyleSubmenus(Forms.ToolStripItemCollection items)
        {
            foreach (Forms.ToolStripItem candidate in items)
            {
                if (candidate is not Forms.ToolStripMenuItem item || item.DropDownItems.Count == 0)
                    continue;
                StylePopup(item.DropDown);
                StyleSubmenus(item.DropDownItems);
            }
        }

        private static void RoundMenu(Forms.ToolStripDropDown menu)
        {
            if (menu.Width < 12 || menu.Height < 12)
                return;
            const int radius = 9;
            using var path = new GraphicsPath();
            var rect = new Rectangle(0, 0, menu.Width - 1, menu.Height - 1);
            int diameter = radius * 2;
            path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            var prior = menu.Region;
            menu.Region = new System.Drawing.Region(path);
            prior?.Dispose();
        }

        private static SolidColorBrush ParseBrush(string value)
            => new((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(value));

        private static LinearGradientBrush Gradient(string top, string bottom)
            => new(ParseBrush(top).Color, ParseBrush(bottom).Color, 90);

        private static DrawingColor DrawingColorTranslator(string value)
            => System.Drawing.ColorTranslator.FromHtml(value);
    }

    internal sealed class RetroColorTable : Forms.ProfessionalColorTable
    {
        private readonly ThemePalette _p;
        public RetroColorTable(ThemePalette palette) => _p = palette;
        private DrawingColor C(string hex) => System.Drawing.ColorTranslator.FromHtml(hex);
        public override DrawingColor ToolStripDropDownBackground => C(_p.Popup);
        public override DrawingColor MenuBorder => C(_p.Border);
        public override DrawingColor MenuItemBorder => C(_p.Border);
        public override DrawingColor MenuItemSelected => C(_p.Hover);
        public override DrawingColor MenuItemSelectedGradientBegin => C(_p.Hover);
        public override DrawingColor MenuItemSelectedGradientEnd => C(_p.Hover);
        public override DrawingColor MenuItemPressedGradientBegin => C(_p.Accent);
        public override DrawingColor MenuItemPressedGradientMiddle => C(_p.Accent);
        public override DrawingColor MenuItemPressedGradientEnd => C(_p.Accent);
        public override DrawingColor ImageMarginGradientBegin => C(_p.Popup);
        public override DrawingColor ImageMarginGradientMiddle => C(_p.Popup);
        public override DrawingColor ImageMarginGradientEnd => C(_p.Popup);
        public override DrawingColor SeparatorDark => C(_p.Border);
        public override DrawingColor SeparatorLight => C(_p.Popup);
    }
}
