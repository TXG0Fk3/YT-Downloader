using System;
using System.IO;
using System.Text.Json;

namespace YT_Downloader.Settings
{
    public class AppSettings
    {
        public byte Theme { get; set; }
        public string DefaultDownloadsPath { get; set; }
        public bool AlwaysAskWhereSave { get; set; }

        // Salva as alterações
        public void SaveNewSettings()
        {
            File.WriteAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YT Downloader\\config.json"), JsonSerializer.Serialize(App.appSettings));
        }
    }
}
