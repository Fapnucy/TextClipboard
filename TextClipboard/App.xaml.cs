using System;
using System.Linq;
using System.Windows;

namespace TextClipboard
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var window = new MainWindow();
            MainWindow = window;
            bool startInBackground = e.Args.Any(arg =>
                string.Equals(arg, "--startup", StringComparison.OrdinalIgnoreCase));

            if (startInBackground)
            {
                // Initialize the hidden WPF window, tray icon and hotkey.
                // No main window on screen when started with Windows.
                window.ShowInTaskbar = false;
                window.WindowState = WindowState.Minimized;
                window.Show();
                window.Hide();
            }
            else
            {
                window.Show();
            }
        }
    }
}
