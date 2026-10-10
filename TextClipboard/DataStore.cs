using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace TextClipboard
{
    public static class DataStore
    {
        // Utworz plik portable.mode obok EXE, aby trzymac dane obok aplikacji.
        // W zwyklym trybie dane sa w %LOCALAPPDATA%\TextClipboard\data.json.
        private static readonly string DirectoryPath =
            File.Exists(Path.Combine(AppContext.BaseDirectory, "portable.mode"))
                ? AppContext.BaseDirectory
                : Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData), "TextClipboard");

        public static string FilePath => Path.Combine(DirectoryPath, "data.json");
        public static string? LastLoadWarning { get; private set; }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public static ClipboardLibrary Load()
        {
            LastLoadWarning = null;

            if (!File.Exists(FilePath))
                return new ClipboardLibrary();

            try
            {
                string json = File.ReadAllText(FilePath, Encoding.UTF8);
                var data = JsonSerializer.Deserialize<ClipboardLibrary>(json, JsonOptions)
                    ?? new ClipboardLibrary();

                data.ThemeName ??= "Olive Classic";
                data.Recent ??= new();
                data.Folders ??= new();
                data.Recent = data.Recent
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .Take(10)
                    .ToList();

                NormalizeFolders(data.Folders);
                return data;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException
                || ex is JsonException)
            {
                LastLoadWarning = "Could not read saved data: " + ex.Message
                    + "\nThe original file was preserved. Back it up before writing any new data.";
                return new ClipboardLibrary();
            }
        }

        private static void NormalizeFolders(System.Collections.Generic.List<TextFolder> folders)
        {
            foreach (TextFolder folder in folders)
            {
                folder.Name ??= "Folder";
                folder.Children ??= new();
                folder.Texts ??= new();
                foreach (SavedText text in folder.Texts)
                {
                    text.Name ??= "Text";
                    text.Content ??= "";
                }
                NormalizeFolders(folder.Children);
            }
        }

        public static void Save(ClipboardLibrary data)
        {
            Directory.CreateDirectory(DirectoryPath);
            string temp = FilePath + ".tmp";
            string json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(temp, json, new UTF8Encoding(false));
            File.Move(temp, FilePath, overwrite: true);
        }
    }
}
