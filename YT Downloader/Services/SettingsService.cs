using System;
using System.IO;
using System.Text.Json;
using YT_Downloader.Models;

namespace YT_Downloader.Services
{
    public class SettingsService
    {
        private const string FileName = "config.json";
        private readonly string _filePath;

        public AppSettings Current { get; private set; }

        public SettingsService()
        {
            _filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "YT Downloader",
                FileName);

            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

            Current = Load();
        }

        private AppSettings Load()
        {
            if (!File.Exists(_filePath))
            {
                return new AppSettings(
                    Theme: "Default",
                    DefaultDownloadsPath: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                    AlwaysAskWhereSave: true
                );
            }

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<AppSettings>(json)!;
        }

        public void Save(AppSettings settings)
        {
            Current = settings;
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}
