using System;
using System.Collections.Generic;

namespace TextClipboard
{
    public sealed class ClipboardLibrary
    {
        public List<string> Recent { get; set; } = new();
        public List<TextFolder> Folders { get; set; } = new();
        public string ThemeName { get; set; } = "Olive Classic";
        // Defaults are also applied when loading pre-1.3.0 data.json.
        public uint HotkeyModifiers { get; set; } = HotkeyGesture.Default.Modifiers;
        public uint HotkeyVirtualKey { get; set; } = HotkeyGesture.Default.VirtualKey;
    }

    public sealed class TextFolder
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "New Folder";
        public List<TextFolder> Children { get; set; } = new();
        public List<SavedText> Texts { get; set; } = new();
    }

    public sealed class SavedText
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "New Text";
        public string Content { get; set; } = string.Empty;
    }
}
