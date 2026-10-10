# 📋 Text Clipboard

**A lightweight, retro-inspired clipboard manager for Windows.**

Text Clipboard is a free Windows application that helps you manage copied text, organize reusable snippets, and paste them quickly into any application.

Inspired by the classic Windows XP interface, it combines a nostalgic design with practical clipboard management features.

**Latest version: v1.3.0**

[⬇️ Download the latest release](https://github.com/Fapnucy/TextClipboard/releases/latest)

---

## ✨ Features

### Clipboard History
- Automatically saves your last 10 copied text entries.
- Displays compact, single-line previews for easier browsing.
- Supports long text entries of up to 1,000,000 characters.
- Open a full-text preview window to read longer entries.
- Easily copy, delete, or clear clipboard history.

### Quick Paste
- Instantly access your clipboard history using a global keyboard shortcut.
- A convenient popup menu appears near your mouse cursor.
- Select an entry to quickly paste its content into your previous application.
- Default shortcut: **Win + Shift + X**.

### Customizable Keyboard Shortcuts
- Change your Quick Paste shortcut in **Settings**.
- Record your preferred keyboard combination.
- The main window automatically displays your current shortcut.
- Shortcut settings are saved between sessions.
- Detects unavailable shortcuts and allows restoring the default.

### Permanent Text Library
- Create folders to organize frequently used text snippets.
- Save important clipboard entries for future use.
- Double-click a saved text to edit its name and content.
- Keep reusable text separate from temporary clipboard history.

### Classic Windows Themes

Choose between five Windows-inspired themes:

- 🟢 Olive Classic
- 🔵 Luna Blue
- ⚪ Silver Frost
- 🩶 Classic Gray
- 🌙 Night Mode

### Additional Features
- System tray integration.
- Optional startup with Windows.
- Lightweight desktop interface.
- Local data storage.
- No account or cloud connection required.

---

## 💻 System Requirements

- **Operating System:** Windows 10 / Windows 11 (64-bit)
- **Framework:** .NET 10
- **Platform:** Windows Desktop (WPF)

## 📥 Installation

1. Open the [Releases page](https://github.com/Fapnucy/TextClipboard/releases).
2. Download the latest `TextClipboard_Setup_1.3.0.exe` installer.
3. Run the installer and follow the setup instructions.
4. Launch Text Clipboard from the Start menu.
5. Start copying text!

**Note:** When updating from an earlier version, close Text Clipboard before running the installer.

## ⌨️ Keyboard Shortcuts

| Shortcut | Action |
|---|---|
| Win + Shift + X | Open Quick Paste (default) |
| Esc | Close the Quick Paste popup |
| Custom shortcut | Configure through Settings |

The default Quick Paste shortcut can be changed at any time.

## 💾 Data Storage

Your clipboard history, saved folders, snippets, and preferences are stored locally on your computer.

Default data location:

`%LOCALAPPDATA%\TextClipboard\data.json`

For backup purposes, copy this file to a safe location while the application is closed.

Text Clipboard does not require an online account or cloud synchronization.

## 🛠️ Building from Source

Text Clipboard is developed using **C#, WPF, and .NET 10**.

To build the application:

1. Clone or download this repository.
2. Open `TextClipboard.sln` in Visual Studio with .NET desktop development support.
3. Restore the required dependencies.
4. Build and run the project.

To create a Windows installer, install Inno Setup and use the included `build-installer.ps1` script.

## 📦 Version History

### v1.3.0
- Customizable Quick Paste keyboard shortcuts.
- New Settings window.
- Dynamic shortcut display.
- Double-click editing for saved text.
- Long-text support and full-text preview.
- Compact clipboard history.

### v1.1.1
- Clipboard history and permanent text folders.
- Quick Paste functionality.
- Five classic Windows-inspired themes.
- System tray support.
- Optional Windows startup.

Visit [GitHub Releases](https://github.com/Fapnucy/TextClipboard/releases) to download previous versions.

---

## 🌍 Documentation

Polish documentation is available in [README_PL.md](README_PL.md).

## 👨‍💻 Author

**Maciej Gosciniak**

GitHub: [@Fapnucy](https://github.com/Fapnucy)

---

**Text Clipboard — Keep your text organized, accessible, and ready to paste.**



## Copyright

Copyright © 2026 Maciej Gosciniak. All rights reserved.

Text Clipboard is not an open-source licensed project.

The source code is publicly available for reference purposes only. Except as permitted by GitHub's Terms of Service or applicable law, copying, modifying, or redistributing the source code requires prior written permission from the copyright holder.
