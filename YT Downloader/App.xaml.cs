using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;


namespace YT_Downloader
{
    public partial class App : Application
    {
        // Variáveis estáticas pq vão ser acessadas de outras classes
        public static CancellationTokenSource cts = new();
        public static MainWindow m_window;
        public static NavigationViewPages.SettingsPage.ConfigFile appConfig;

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
                appConfig = JsonSerializer.Deserialize<NavigationViewPages.SettingsPage.ConfigFile>(jsonString);
            }
            // Se não, cria um com configurações padrão
            else
            {
                appConfig = new NavigationViewPages.SettingsPage.ConfigFile
                {
                    AppTheme = 2,
                    DefaultDownloadsPath = $"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")}\\YT Downloader",
                    AlwaysAskWhereSave = true
                };
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YT Downloader"));
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YT Downloader\\config.json"), JsonSerializer.Serialize(appConfig));
            }

            Debug.WriteLine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YT Downloader\\config.json"));

            // Carrega a janela principal
            m_window = new MainWindow();
            m_window.Activate();
        }
    }
}
