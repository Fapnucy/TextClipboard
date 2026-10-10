using System.Windows;

namespace TextClipboard
{
    public partial class TextEditDialog : Window
    {
        private readonly bool _isFolder;

        public string ItemName { get; private set; } = "";
        public string ItemContent { get; private set; } = "";

        public TextEditDialog(string title, string name, string content, bool isFolder)
        {
            InitializeComponent();
            Title = title;
            _isFolder = isFolder;
            NameBox.Text = name;
            ContentBox.Text = content;

            if (isFolder)
            {
                ContentSection.Visibility = Visibility.Collapsed;
                Height = 190;
                MinHeight = 190;
                MaxHeight = 190;
            }
            Loaded += (_, _) =>
            {
                NameBox.Focus();
                NameBox.SelectAll();
            };
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string name = NameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show(this, "Enter a name.", "Text Clipboard",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!_isFolder && string.IsNullOrEmpty(ContentBox.Text))
            {
                MessageBox.Show(this, "Enter text content.", "Text Clipboard",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ItemName = name;
            ItemContent = ContentBox.Text;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
