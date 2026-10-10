using System;
using System.Windows.Input;
using System.Windows.Interop;

namespace TextClipboard
{
    /// <summary>
    /// A Windows RegisterHotKey compatible shortcut. MOD_NOREPEAT is added
    /// when registering, and is not stored in the user's settings.
    /// </summary>
    public readonly record struct HotkeyGesture(uint Modifiers, uint VirtualKey)
    {
        public const uint Alt = 0x0001;
        public const uint Control = 0x0002;
        public const uint Shift = 0x0004;
        public const uint Windows = 0x0008;
        public const uint ValidModifierMask = Alt | Control | Shift | Windows;
        public const uint NoRepeat = 0x4000;

        public static HotkeyGesture Default => new(Windows | Shift, 0x58); // Win + Shift + X

        public static bool TryCreate(Key key, ModifierKeys modifiers,
            out HotkeyGesture gesture, out string error)
        {
            gesture = default;
            uint virtualKey = (uint)KeyInterop.VirtualKeyFromKey(key);
            uint flags = 0;
            if ((modifiers & ModifierKeys.Control) != 0) flags |= Control;
            if ((modifiers & ModifierKeys.Alt) != 0) flags |= Alt;
            if ((modifiers & ModifierKeys.Shift) != 0) flags |= Shift;
            if ((modifiers & ModifierKeys.Windows) != 0) flags |= Windows;
            gesture = new HotkeyGesture(flags, virtualKey);
            return gesture.IsValid(out error);
        }

        public bool IsValid(out string error)
        {
            if (Modifiers == 0 || (Modifiers & ~ValidModifierMask) != 0)
            {
                error = "Use at least one modifier: Ctrl, Shift, Alt or Win.";
                return false;
            }
            // On international keyboard layouts AltGr is often delivered as
            // Ctrl+Alt; such global shortcuts can block normal text input.
            if ((Modifiers & (Control | Alt)) == (Control | Alt))
            {
                error = "Ctrl + Alt shortcuts are disabled to protect AltGr characters (e.g. Polish ź).";
                return false;
            }
            if (!IsSupportedKey(VirtualKey))
            {
                error = "Choose a letter (A-Z), number (0-9), function key (F1-F24) or navigation key.";
                return false;
            }
            error = string.Empty;
            return true;
        }

        private static bool IsSupportedKey(uint key)
        {
            return (key >= 0x41 && key <= 0x5A) // A-Z
                || (key >= 0x30 && key <= 0x39) // 0-9
                || (key >= 0x70 && key <= 0x87) // F1-F24
                || (key >= 0x60 && key <= 0x69) // Numpad 0-9
                || key == 0x21 || key == 0x22  // PageUp, PageDown
                || key == 0x23 || key == 0x24  // End, Home
                || key == 0x25 || key == 0x26 || key == 0x27 || key == 0x28 // Arrows
                || key == 0x2D || key == 0x2E; // Insert, Delete
        }

        public string ToDisplayString()
        {
            string label = string.Empty;
            if ((Modifiers & Control) != 0) label += "Ctrl + ";
            if ((Modifiers & Alt) != 0) label += "Alt + ";
            if ((Modifiers & Shift) != 0) label += "Shift + ";
            if ((Modifiers & Windows) != 0) label += "Win + ";
            label += FormatKey(VirtualKey);
            return label;
        }

        private static string FormatKey(uint vk)
        {
            if (vk >= 0x41 && vk <= 0x5A) return ((char)vk).ToString();
            if (vk >= 0x30 && vk <= 0x39) return ((char)vk).ToString();
            if (vk >= 0x70 && vk <= 0x87) return "F" + (vk - 0x6F);
            if (vk >= 0x60 && vk <= 0x69) return "Num " + (vk - 0x60);
            return vk switch
            {
                0x21 => "Page Up", 0x22 => "Page Down", 0x23 => "End",
                0x24 => "Home", 0x25 => "Left", 0x26 => "Up",
                0x27 => "Right", 0x28 => "Down", 0x2D => "Insert",
                0x2E => "Delete", _ => "Key " + vk
            };
        }
    }
}
