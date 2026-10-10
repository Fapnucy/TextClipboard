using System;
using System.Windows;

namespace TextClipboard
{
    public partial class TextPreviewWindow : Window
    {
        public event Action<string>? CopyRequested;

        public TextPreviewWindow(string fullText)
        {
            InitializeComponent();
            PreviewTextBox.Text = fullText;
            CharacterCountText.Text = $"{fullText.Length:N0} characters - full text shown below";
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            CopyRequested?.Invoke(PreviewTextBox.Text);
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
