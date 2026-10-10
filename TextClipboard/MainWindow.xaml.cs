using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace TextClipboard
{
    public partial class MainWindow : Window
    {
        private const int WM_CLIPBOARDUPDATE = 0x031D;
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 123;
        private const int ESC_HOTKEY_ID = 124;
        private const int ALTERNATE_HOTKEY_ID = 125;
        private const uint VK_ESCAPE = 0x1B;
        private const uint MOD_NOREPEAT = HotkeyGesture.NoRepeat;
        private const byte VK_CONTROL = 0x11;
        private const byte VK_V = 0x56;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const int HISTORY_LIMIT = 10;
        private const int MAX_TEXT_LENGTH = 1_000_000;

        private readonly ClipboardLibrary _library;
        private readonly ObservableCollection<string> _recent = new();
        private HwndSource? _source;
        private Forms.NotifyIcon? _trayIcon;
        private Forms.ContextMenuStrip? _trayMenu;
        private Forms.ContextMenuStrip? _quickMenu;
        private bool _exitRequested;
        private bool _hotkeyRegistered;
        private int _activeHotkeyId = HOTKEY_ID;
        private HotkeyGesture _currentHotkey;
        private bool _clipboardListenerRegistered;
        private bool _storageWritable;
        private bool _quickMenuOpen;
        private bool _escapeHotkeyRegistered;
        private System.Drawing.Icon? _ownedTrayIcon;
        private uint _ownClipboardSequence;
        private IntPtr _pasteTargetWindow;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RegisterHotKey(
            IntPtr hwnd, int id, uint modifiers, uint key);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnregisterHotKey(IntPtr hwnd, int id);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern uint GetClipboardSequenceNumber();

        [DllImport("user32.dll")]
        private static extern void keybd_event(
            byte virtualKey, byte scanCode, uint flags, UIntPtr extraInfo);

        public MainWindow()
        {
            _library = DataStore.Load();
            _currentHotkey = new HotkeyGesture(_library.HotkeyModifiers, _library.HotkeyVirtualKey);
            ThemeManager.ApplyTheme(_library.ThemeName);
            InitializeComponent();
            RefreshThemeMenuChecks();
            UpdateHotkeyDisplay();
            _storageWritable = DataStore.LastLoadWarning == null;

            foreach (string value in _library.Recent.Take(HISTORY_LIMIT))
                _recent.Add(value);

            HistoryList.ItemsSource = _recent;
            RebuildFolderTree();
            RefreshControls();

            SourceInitialized += MainWindow_SourceInitialized;
            StateChanged += MainWindow_StateChanged;
            Closing += MainWindow_Closing;
            Closed += MainWindow_Closed;
            Loaded += (_, _) =>
            {
                if (DataStore.LastLoadWarning != null)
                {
                    MessageBox.Show(this, DataStore.LastLoadWarning,
                        "Text Clipboard - Data Warning", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            };

            CreateTrayIcon();
        }

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(hwnd);
            _source?.AddHook(WndProc);

            _clipboardListenerRegistered = AddClipboardFormatListener(hwnd);
            _hotkeyRegistered = RegisterHotKey(hwnd, _activeHotkeyId,
                _currentHotkey.Modifiers | MOD_NOREPEAT, _currentHotkey.VirtualKey);

            if (!_clipboardListenerRegistered)
                MessageBox.Show(this, "Could not monitor the Windows clipboard.",
                    "Text Clipboard", MessageBoxButton.OK, MessageBoxImage.Warning);

            if (!_hotkeyRegistered)
                MessageBox.Show(this,
                    "Cannot register " + _currentHotkey.ToDisplayString()
                    + ". Another application may be using this shortcut. Change it in Settings.",
                    "Text Clipboard", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam,
            IntPtr lParam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                uint sequence = GetClipboardSequenceNumber();
                if (sequence != _ownClipboardSequence)
                    _ = ReadClipboardWithRetryAsync(sequence);
            }

            if (msg == WM_HOTKEY && _hotkeyRegistered && wParam.ToInt32() == _activeHotkeyId)
            {
                ToggleQuickMenu();
                handled = true;
            }

            if (msg == WM_HOTKEY && wParam.ToInt32() == ESC_HOTKEY_ID)
            {
                _quickMenu?.Close();
                handled = true;
            }

            return IntPtr.Zero;
        }

        private async Task ReadClipboardWithRetryAsync(uint originalSequence)
        {
            // Inne aplikacje potrafia przez chwile blokowac dostep do schowka.
            for (int attempt = 0; attempt < 4; attempt++)
            {
                if (originalSequence == _ownClipboardSequence)
                    return;

                try
                {
                    if (!Clipboard.ContainsText(TextDataFormat.UnicodeText))
                        return;

                    string value = Clipboard.GetText(TextDataFormat.UnicodeText);
                    if (string.IsNullOrWhiteSpace(value))
                        return;

                    if (value.Length > MAX_TEXT_LENGTH)
                    {
                        StatusLabel.Text = $"Text exceeds the {MAX_TEXT_LENGTH:N0} character limit and was not saved.";
                        return;
                    }

                    AddRecentText(value);
                    return;
                }
                catch (ExternalException)
                {
                    await Task.Delay(80);
                }
            }
        }

        private void AddRecentText(string value)
        {
            int position = _recent.IndexOf(value);
            if (position == 0)
                return;
            if (position > 0)
                _recent.RemoveAt(position);

            _recent.Insert(0, value);
            while (_recent.Count > HISTORY_LIMIT)
                _recent.RemoveAt(_recent.Count - 1);

            SaveAll();
            RefreshControls();
        }

        private void CopyText(string value)
        {
            try
            {
                Clipboard.SetText(value, TextDataFormat.UnicodeText);
                _ownClipboardSequence = GetClipboardSequenceNumber();
                StatusLabel.Text = "Text copied to clipboard.";
            }
            catch (ExternalException)
            {
                MessageBox.Show(this, "The clipboard is busy. Please try again.",
                    "Text Clipboard", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool SaveAll()
        {
            if (!_storageWritable)
            {
                StatusLabel.Text = "Saving disabled: original data could not be read safely.";
                return false;
            }

            try
            {
                _library.Recent = _recent.ToList();
                DataStore.Save(_library);
                return true;
            }
            catch (Exception ex) when (ex is System.IO.IOException
                || ex is UnauthorizedAccessException)
            {
                StatusLabel.Text = "Could not save data: " + ex.Message;
                return false;
            }
        }

        private void RefreshControls()
        {
            HistoryCounter.Text = $"{_recent.Count} / {HISTORY_LIMIT}";
            ClearRecentButton.IsEnabled = _recent.Count > 0;
            CopyRecentButton.IsEnabled = HistoryList.SelectedItem is string;
            PreviewRecentButton.IsEnabled = HistoryList.SelectedItem is string;
            DeleteRecentButton.IsEnabled = HistoryList.SelectedItem is string;

            TextFolder? folder = GetSelectedFolder();
            SaveRecentButton.IsEnabled = HistoryList.SelectedItem is string && folder != null;
            AddTextButton.IsEnabled = folder != null;
            EditTreeButton.IsEnabled = FoldersTree.SelectedItem is TreeViewItem;
            DeleteTreeButton.IsEnabled = EditTreeButton.IsEnabled;


        }

        // Clicking a selected item again clears selection; clicking empty space also clears it.
        private static T? FindParent<T>(DependencyObject? element) where T : DependencyObject
        {
            while (element != null)
            {
                if (element is T match)
                    return match;
                element = VisualTreeHelper.GetParent(element);
            }
            return null;
        }

        private void HistoryList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Leave double clicks intact: they copy text into the clipboard.
            if (e.ClickCount > 1)
                return;
            var item = FindParent<ListBoxItem>(e.OriginalSource as DependencyObject);
            if (item == null)
            {
                HistoryList.SelectedItem = null;
            }
            else if (item.IsSelected)
            {
                HistoryList.SelectedItem = null;
                e.Handled = true;
            }
        }

        private void FoldersTree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Do not deselect an item during a double-click used for editing.
            if (e.ClickCount > 1)
                return;
            // Preserve normal clicking on [+] / [-] expanders.
            if (FindParent<System.Windows.Controls.Primitives.ToggleButton>(
                e.OriginalSource as DependencyObject) != null)
                return;

            var item = FindParent<TreeViewItem>(e.OriginalSource as DependencyObject);
            if (item == null)
            {
                if (SelectedNode != null)
                    SelectedNode.IsSelected = false;
            }
            else if (item.IsSelected)
            {
                item.IsSelected = false;
                e.Handled = true;
            }
        }

        private void ThemeMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem item || item.Tag is not string themeName)
                return;

            _library.ThemeName = themeName;
            ThemeManager.ApplyTheme(themeName);
            RefreshThemeMenuChecks();
            SaveAll();
        }

        private void RefreshThemeMenuChecks()
        {
            foreach (MenuItem item in ThemeMenu.Items.OfType<MenuItem>())
                item.IsChecked = string.Equals(item.Tag as string,
                    ThemeManager.Current.Name, StringComparison.Ordinal);
        }

        private void UpdateHotkeyDisplay()
        {
            HotkeyHintText.Text = _currentHotkey.ToDisplayString();
            if (_trayIcon != null)
                _trayIcon.Text = "Text Clipboard - " + _currentHotkey.ToDisplayString();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow(_currentHotkey, TryApplyHotkey) { Owner = this };
            settings.ShowDialog();
        }

        // Register the candidate under a different ID first; never leave the
        // app without the old shortcut just because the new one is occupied.
        private string? TryApplyHotkey(HotkeyGesture next)
        {
            if (!next.IsValid(out string invalidReason))
                return invalidReason;

            if (next == _currentHotkey && _hotkeyRegistered)
                return null;

            if (_source == null)
                return "The application window is not ready yet. Please try again.";

            int candidateId = _hotkeyRegistered
                ? (_activeHotkeyId == HOTKEY_ID ? ALTERNATE_HOTKEY_ID : HOTKEY_ID)
                : _activeHotkeyId;

            bool registered = RegisterHotKey(_source.Handle, candidateId,
                next.Modifiers | MOD_NOREPEAT, next.VirtualKey);
            if (!registered)
                return "Windows could not register " + next.ToDisplayString()
                    + ". The shortcut may be reserved or already used by another application."
                    + " Your previous shortcut has been kept.";

            // Persist before releasing the original hotkey. If the data file
            // cannot be saved, roll back the candidate registration.
            uint priorModifiers = _library.HotkeyModifiers;
            uint priorVirtualKey = _library.HotkeyVirtualKey;
            _library.HotkeyModifiers = next.Modifiers;
            _library.HotkeyVirtualKey = next.VirtualKey;

            if (!SaveAll())
            {
                _library.HotkeyModifiers = priorModifiers;
                _library.HotkeyVirtualKey = priorVirtualKey;
                UnregisterHotKey(_source.Handle, candidateId);
                return "Could not save the new shortcut to disk."
                    + " Your previous shortcut is still active."
                    + " See the status bar for details.";
            }

            if (_hotkeyRegistered)
                UnregisterHotKey(_source.Handle, _activeHotkeyId);

            _activeHotkeyId = candidateId;
            _hotkeyRegistered = true;
            _currentHotkey = next;
            UpdateHotkeyDisplay();
            StatusLabel.Text = "Quick Paste shortcut changed to " + next.ToDisplayString() + ".";
            return null;
        }

        private void HelpAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this,
                "Text Clipboard\n" +
                "Version: 1.3.0 — Retro Edition\n" +
                "Created: 10 October 2026\n" +
                "Author: Maciej Gosciniak\n\n" +
                "Global shortcut: " + _currentHotkey.ToDisplayString() + "\n" +
                "Your history and saved folders are stored locally.",
                "About Text Clipboard", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void FileExit_Click(object sender, RoutedEventArgs e) => ExitApplication();

        private void Minimize_Click(object sender, RoutedEventArgs e)
            => WindowState = System.Windows.WindowState.Minimized;

        private void Maximize_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState == System.Windows.WindowState.Maximized
                ? System.Windows.WindowState.Normal : System.Windows.WindowState.Maximized;

        private void Hide_Click(object sender, RoutedEventArgs e) => Hide();

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;
            if (e.ClickCount == 2)
                WindowState = WindowState == System.Windows.WindowState.Maximized
                    ? System.Windows.WindowState.Normal : System.Windows.WindowState.Maximized;
            else
                DragMove();
        }

        private void HistoryList_SelectionChanged(
            object sender, SelectionChangedEventArgs e) => RefreshControls();

        private void HistoryList_MouseDoubleClick(
            object sender, MouseButtonEventArgs e)
        {
            // Double-click shows the full text without copying it immediately.
            PreviewSelectedRecent();
        }

        private void PreviewRecent_Click(object sender, RoutedEventArgs e)
        {
            PreviewSelectedRecent();
        }

        private void PreviewSelectedRecent()
        {
            if (HistoryList.SelectedItem is not string fullText)
                return;

            var preview = new TextPreviewWindow(fullText) { Owner = this };
            preview.CopyRequested += CopyText;
            preview.ShowDialog();
        }

        private void CopyRecent_Click(object sender, RoutedEventArgs e)
        {
            if (HistoryList.SelectedItem is string value)
                CopyText(value);
        }

        private void DeleteRecent_Click(object sender, RoutedEventArgs e)
        {
            if (HistoryList.SelectedItem is not string value)
                return;
            _recent.Remove(value);
            SaveAll();
            RefreshControls();
        }

        private void ClearRecent_Click(object sender, RoutedEventArgs e)
        {
            if (_recent.Count == 0)
                return;
            if (MessageBox.Show(this, "Delete the recent clipboard history?\n"
                + "Saved texts inside folders will not be deleted.",
                "Clear History", MessageBoxButton.YesNo,
                MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            _recent.Clear();
            SaveAll();
            RefreshControls();
        }

        private void SaveRecent_Click(object sender, RoutedEventArgs e)
        {
            TextFolder? folder = GetSelectedFolder();
            if (folder == null || HistoryList.SelectedItem is not string value)
                return;

            var editor = new TextEditDialog("Save Text to Folder",
                GuessTitle(value), value, isFolder: false) { Owner = this };
            if (editor.ShowDialog() != true)
                return;

            folder.Texts.Add(new SavedText
            {
                Name = editor.ItemName,
                Content = editor.ItemContent
            });
            RebuildFolderTree();
            SaveAll();
            StatusLabel.Text = "Text saved to folder: " + folder.Name;
        }

        private static string GuessTitle(string value)
        {
            string line = value.Replace('\r', ' ').Replace('\n', ' ').Trim();
            return line.Length > 45 ? line.Substring(0, 45) + "..." : line;
        }

        private void RebuildFolderTree()
        {
            FoldersTree.Items.Clear();
            foreach (TextFolder folder in _library.Folders)
                FoldersTree.Items.Add(CreateFolderTreeItem(folder));
            RefreshControls();
        }

        private static TreeViewItem CreateFolderTreeItem(TextFolder folder)
        {
            var item = new TreeViewItem
            {
                Header = "📁 " + folder.Name,
                Tag = folder,
                IsExpanded = true
            };

            foreach (TextFolder child in folder.Children)
                item.Items.Add(CreateFolderTreeItem(child));
            foreach (SavedText text in folder.Texts)
            {
                item.Items.Add(new TreeViewItem
                {
                    Header = "📝 " + text.Name,
                    Tag = text,
                    ToolTip = text.Content.Length > 250
                        ? text.Content.Substring(0, 250) + "..." : text.Content
                });
            }
            return item;
        }

        private TreeViewItem? SelectedNode => FoldersTree.SelectedItem as TreeViewItem;

        private TextFolder? GetSelectedFolder()
        {
            TreeViewItem? node = SelectedNode;
            if (node?.Tag is TextFolder folder)
                return folder;
            if (node?.Tag is SavedText && node.Parent is TreeViewItem parent)
                return parent.Tag as TextFolder;
            return null;
        }

        private void FoldersTree_SelectedItemChanged(
            object sender, RoutedPropertyChangedEventArgs<object> e) => RefreshControls();

        private void FoldersTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            // Only saved text entries open the editor; double-clicking a
            // folder continues to expand/collapse it normally.
            var node = FindParent<TreeViewItem>(e.OriginalSource as DependencyObject);
            if (node?.Tag is SavedText savedText)
            {
                e.Handled = true;
                EditSavedText(savedText);
            }
        }

        private void EditSavedText(SavedText savedText)
        {
            var editor = new TextEditDialog("Edit Text", savedText.Name,
                savedText.Content, isFolder: false) { Owner = this };
            if (editor.ShowDialog() != true)
                return;

            savedText.Name = editor.ItemName;
            savedText.Content = editor.ItemContent;
            RebuildFolderTree();
            SaveAll();
            StatusLabel.Text = "Saved text updated: " + savedText.Name;
        }

        private void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            var editor = new TextEditDialog("New Folder", "New Folder", "", isFolder: true)
            {
                Owner = this
            };
            if (editor.ShowDialog() != true)
                return;

            var folder = new TextFolder { Name = editor.ItemName };
            TextFolder? parent = GetSelectedFolder();
            if (parent != null)
                parent.Children.Add(folder);
            else
                _library.Folders.Add(folder);

            RebuildFolderTree();
            SaveAll();
        }

        private void AddText_Click(object sender, RoutedEventArgs e)
        {
            TextFolder? folder = GetSelectedFolder();
            if (folder == null)
                return;

            var editor = new TextEditDialog("New Saved Text", "", "", isFolder: false)
            {
                Owner = this
            };
            if (editor.ShowDialog() != true)
                return;

            folder.Texts.Add(new SavedText { Name = editor.ItemName, Content = editor.ItemContent });
            RebuildFolderTree();
            SaveAll();
        }

        private void EditTree_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem? node = SelectedNode;
            if (node?.Tag is TextFolder folder)
            {
                var editor = new TextEditDialog("Rename Folder", folder.Name, "", true)
                {
                    Owner = this
                };
                if (editor.ShowDialog() == true)
                    folder.Name = editor.ItemName;
                else
                    return;
            }
            else if (node?.Tag is SavedText text)
            {
                EditSavedText(text);
                return;
            }
            else
                return;

            RebuildFolderTree();
            SaveAll();
        }

        private void DeleteTree_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem? node = SelectedNode;
            if (node == null)
                return;

            if (node.Tag is TextFolder folder)
            {
                if (MessageBox.Show(this,
                    "Delete folder '" + folder.Name + "' and all its subfolders and saved texts?",
                    "Delete Folder", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                    MessageBoxResult.No) != MessageBoxResult.Yes)
                    return;

                if (node.Parent is TreeViewItem parent && parent.Tag is TextFolder parentFolder)
                    parentFolder.Children.Remove(folder);
                else
                    _library.Folders.Remove(folder);
            }
            else if (node.Tag is SavedText text)
            {
                if (MessageBox.Show(this, "Delete text '" + text.Name + "'?",
                    "Delete Text", MessageBoxButton.YesNo, MessageBoxImage.Question,
                    MessageBoxResult.No) != MessageBoxResult.Yes)
                    return;

                if (node.Parent is TreeViewItem parent && parent.Tag is TextFolder parentFolder)
                    parentFolder.Texts.Remove(text);
            }

            RebuildFolderTree();
            SaveAll();
        }

        // -------- Quick Paste context menu: historia i rozwiniete foldery --------

        private void ToggleQuickMenu()
        {
            if (_quickMenuOpen && _quickMenu != null)
            {
                _quickMenu.Close();
                return;
            }

            _pasteTargetWindow = GetForegroundWindow();
            BuildQuickMenu();

            if (_quickMenu == null)
                return;

            _quickMenuOpen = true;
            // Register Escape temporarily, so it also closes open submenus.
            if (_source != null)
            {
                _escapeHotkeyRegistered = RegisterHotKey(
                    _source.Handle, ESC_HOTKEY_ID, MOD_NOREPEAT, VK_ESCAPE);
            }
            _quickMenu.Show(Forms.Cursor.Position);
        }

        private void BuildQuickMenu()
        {
            if (_quickMenu != null)
            {
                _quickMenu.Dispose();
                _quickMenu = null;
            }

            var menu = new Forms.ContextMenuStrip
            {
                ShowImageMargin = false,
                ShowCheckMargin = false,
                MaximumSize = new System.Drawing.Size(490, 830)
            };
            ThemeManager.StylePopup(menu);
            menu.KeyDown += (_, args) =>
            {
                if (args.KeyCode == Forms.Keys.Escape)
                {
                    menu.Close();
                    args.Handled = true;
                }
            };
            menu.Closed += (_, _) =>
            {
                _quickMenuOpen = false;
                if (_escapeHotkeyRegistered && _source != null)
                {
                    UnregisterHotKey(_source.Handle, ESC_HOTKEY_ID);
                    _escapeHotkeyRegistered = false;
                }
            };

            menu.Items.Add(new Forms.ToolStripMenuItem("RECENT COPIES")
            {
                Enabled = false
            });

            if (_recent.Count == 0)
            {
                menu.Items.Add(new Forms.ToolStripMenuItem("(history is empty)")
                {
                    Enabled = false
                });
            }
            else
            {
                for (int i = 0; i < _recent.Count; i++)
                {
                    string fullText = _recent[i];
                    var item = new Forms.ToolStripMenuItem(
                        (i + 1).ToString() + ".  " + Preview(fullText));
                    item.ToolTipText = fullText.Length > 500
                        ? fullText.Substring(0, 500) + "..." : fullText;
                    item.Click += (_, _) => SelectForQuickPaste(fullText);
                    menu.Items.Add(item);
                }
            }

            menu.Items.Add(new Forms.ToolStripSeparator());
            var folderRoot = new Forms.ToolStripMenuItem("📁 Folders");
            if (_library.Folders.Count == 0)
            {
                folderRoot.DropDownItems.Add(new Forms.ToolStripMenuItem("(no folders)")
                {
                    Enabled = false
                });
            }
            else
            {
                foreach (TextFolder folder in _library.Folders)
                    folderRoot.DropDownItems.Add(CreateFolderMenuItem(folder));
            }
            menu.Items.Add(folderRoot);

            menu.Items.Add(new Forms.ToolStripSeparator());
            var open = new Forms.ToolStripMenuItem("Open Text Clipboard");
            open.Click += (_, _) => Dispatcher.BeginInvoke(new Action(ShowMainWindow));
            menu.Items.Add(open);


            ThemeManager.StyleSubmenus(menu.Items);
            _quickMenu = menu;
        }

        private Forms.ToolStripMenuItem CreateFolderMenuItem(TextFolder folder)
        {
            var item = new Forms.ToolStripMenuItem(EscapeAmpersands(folder.Name));

            foreach (TextFolder subFolder in folder.Children)
                item.DropDownItems.Add(CreateFolderMenuItem(subFolder));

            foreach (SavedText text in folder.Texts)
            {
                string value = text.Content;
                var textItem = new Forms.ToolStripMenuItem(EscapeAmpersands(text.Name));
                textItem.ToolTipText = value.Length > 500
                    ? value.Substring(0, 500) + "..." : value;
                textItem.Click += (_, _) => SelectForQuickPaste(value);
                item.DropDownItems.Add(textItem);
            }

            if (item.DropDownItems.Count == 0)
            {
                item.DropDownItems.Add(new Forms.ToolStripMenuItem("(empty folder)")
                {
                    Enabled = false
                });
            }

            return item;
        }

        private static string Preview(string text)
        {
            string oneLine = text.Replace("\r", "").Replace("\n", " ↵ ")
                                 .Replace("\t", " ⇥ ").Trim();
            if (oneLine.Length > 65)
                oneLine = oneLine.Substring(0, 65) + "...";
            return EscapeAmpersands(oneLine);
        }

        private static string EscapeAmpersands(string text) => text.Replace("&", "&&");

        private void SelectForQuickPaste(string value)
        {
            IntPtr target = _pasteTargetWindow;
            _quickMenu?.Close();
            Dispatcher.BeginInvoke(new Action(() => _ = PasteIntoPreviousWindowAsync(value, target)),
                DispatcherPriority.Background);
        }

        private async Task PasteIntoPreviousWindowAsync(string value, IntPtr target)
        {
            try
            {
                Clipboard.SetText(value, TextDataFormat.UnicodeText);
                _ownClipboardSequence = GetClipboardSequenceNumber();
            }
            catch (ExternalException)
            {
                StatusLabel.Text = "Clipboard is busy; paste did not complete.";
                return;
            }

            IntPtr myWindow = new WindowInteropHelper(this).Handle;
            if (target == IntPtr.Zero || target == myWindow)
                return;

            // Dopiero po schowaniu menu przywracamy poprzednie okno.
            await Task.Delay(100);
            if (!SetForegroundWindow(target))
            {
                StatusLabel.Text = "Could not focus the target window. Text is in clipboard (Ctrl+V).";
                return;
            }

            await Task.Delay(140);
            if (GetForegroundWindow() != target)
            {
                StatusLabel.Text = "Target window not active. Use Ctrl+V to paste manually.";
                return;
            }

            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
            keybd_event(VK_V, 0, 0, UIntPtr.Zero);
            keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        // -------- Ikona obok zegara --------

        private void CreateTrayIcon()
        {
            _trayMenu = new Forms.ContextMenuStrip();
            var open = new Forms.ToolStripMenuItem("Open Text Clipboard");
            open.Click += (_, _) => Dispatcher.BeginInvoke(new Action(ShowMainWindow));
            _trayMenu.Items.Add(open);

            _trayMenu.Items.Add(new Forms.ToolStripSeparator());

            var exit = new Forms.ToolStripMenuItem("Exit Application");
            exit.Click += (_, _) => Dispatcher.BeginInvoke(new Action(ExitApplication));
            _trayMenu.Items.Add(exit);

            // Use the custom icon embedded into TextClipboard.exe.
            string? executablePath = Environment.ProcessPath;
            _ownedTrayIcon = !string.IsNullOrEmpty(executablePath)
                ? System.Drawing.Icon.ExtractAssociatedIcon(executablePath)
                : null;
            var icon = _ownedTrayIcon ?? System.Drawing.SystemIcons.Application;
            Icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                System.Windows.Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            _trayIcon = new Forms.NotifyIcon
            {
                Icon = icon,
                Text = "Text Clipboard - " + _currentHotkey.ToDisplayString(),
                ContextMenuStrip = _trayMenu,
                Visible = true
            };
            _trayIcon.MouseDoubleClick += (_, e) =>
            {
                if (e.Button == Forms.MouseButtons.Left)
                    Dispatcher.BeginInvoke(new Action(ShowMainWindow));
            };
            RefreshControls();
        }

        private void ShowMainWindow()
        {
            ShowInTaskbar = true;
            Show();
            WindowState = System.Windows.WindowState.Normal;
            Activate();
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                Hide();
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (!_exitRequested)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void ExitApplication()
        {
            _exitRequested = true;
            Close();
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            if (_source != null)
            {
                if (_escapeHotkeyRegistered)
                {
                    UnregisterHotKey(_source.Handle, ESC_HOTKEY_ID);
                    _escapeHotkeyRegistered = false;
                }
                if (_hotkeyRegistered)
                    UnregisterHotKey(_source.Handle, _activeHotkeyId);
                if (_clipboardListenerRegistered)
                    RemoveClipboardFormatListener(_source.Handle);
                _source.RemoveHook(WndProc);
            }

            _quickMenu?.Dispose();
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }
            _trayMenu?.Dispose();
            _ownedTrayIcon?.Dispose();
            _ownedTrayIcon = null;
        }
    }
}
