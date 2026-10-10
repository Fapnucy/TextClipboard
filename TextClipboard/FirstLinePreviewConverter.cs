using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TextClipboard
{
    // The full text remains unchanged in the history. Only its display is shortened.
    public sealed class FirstLinePreviewConverter : IValueConverter
    {
        private const int MaximumPreviewCharacters = 140;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string text || text.Length == 0)
                return "(empty)";

            int endOfFirstLine = text.IndexOfAny(new[] { '\r', '\n' });
            bool hasMoreLines = endOfFirstLine >= 0;
            int lineLength = hasMoreLines ? endOfFirstLine : text.Length;
            bool tooLong = lineLength > MaximumPreviewCharacters;
            string firstLine = text.Substring(0, Math.Min(lineLength, MaximumPreviewCharacters))
                .Replace('\t', ' ').Trim();

            if (firstLine.Length == 0)
                firstLine = "(blank first line)";

            return firstLine + ((tooLong || hasMoreLines) ? " ..." : "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }
}
