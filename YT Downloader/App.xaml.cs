using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;


namespace YT_Downloader
{
    public partial class App : Application
    {
        // Variáveis estáticas pq vão ser acessadas de outras classes
        public static CancellationTokenSource cts = new();
        public static MainWindow mainWindow;
        public static Settings.AppSettings appSettings;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Verifica se já existe um arquivo de configuração
            // Se sim, carrega ele
            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YT Downloader\\config.json")))
            {
                string jsonString = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YT Downloader\\config.json"));
                appSettings = JsonSerializer.Deserialize<Settings.AppSettings>(jsonString);
            }
            // Se não, cria um com configurações padrão
            else
            {
                appSettings = new Settings.AppSettings
                {
                    Theme = 2,
                    DefaultDownloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                    AlwaysAskWhereSave = true
                };
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YT Downloader"));
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YT Downloader\\config.json"), JsonSerializer.Serialize(appSettings));
            }

            // Carrega a janela principal
            mainWindow = new MainWindow();
            mainWindow.Activate();
        }
    }
}
