using System;
using System.Windows;
using System.Windows.Input;

namespace TextClipboard
{
    public partial class SettingsWindow : Window
    {
        private readonly Func<HotkeyGesture, string?> _applyShortcut;
        private HotkeyGesture _pending;

        // Returns null on successful registration/persistence,
        // or a message describing why the current shortcut was preserved.
        public SettingsWindow(HotkeyGesture current, Func<HotkeyGesture, string?> applyShortcut)
        {
            _pending = current;
            _applyShortcut = applyShortcut;
            InitializeComponent();
            UpdateDisplay();
            Loaded += (_, _) => ShortcutBox.Focus();
        }

        private void UpdateDisplay()
        {
            ShortcutBox.Text = _pending.ToDisplayString();
        }

        private void ShortcutBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShortcutBox.Focus();
        }

        private void ShortcutBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            // Modifier-only presses don't replace the current shortcut.
            if (key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LeftAlt || key == Key.RightAlt ||
                key == Key.LWin || key == Key.RWin ||
                key == Key.None)
                return;

            if (!HotkeyGesture.TryCreate(key, Keyboard.Modifiers,
                out HotkeyGesture proposed, out string error))
            {
                HintLabel.Text = error;
                return;
            }

            _pending = proposed;
            UpdateDisplay();
            HintLabel.Text = "Click Save to activate this shortcut.";
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            _pending = HotkeyGesture.Default;
            UpdateDisplay();
            HintLabel.Text = "Default shortcut selected. Click Save to apply it.";
            ShortcutBox.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!_pending.IsValid(out string error))
            {
                MessageBox.Show(this, error, "Invalid shortcut",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string? result = _applyShortcut(_pending);
            if (result != null)
            {
                MessageBox.Show(this, result, "Shortcut not changed",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
