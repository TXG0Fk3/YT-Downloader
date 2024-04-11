using Microsoft.UI.Xaml;
using System;
using System.Threading;
using System.Text.Json;
using System.IO;
using YT_Downloader.NavigationViewPages;


namespace YT_Downloader
{
    public partial class App : Application
    {
        // Variáveis estáticas pq vão ser acessadas de outras classes
        public static CancellationTokenSource cts = new CancellationTokenSource();
        public static MainWindow m_window;
        public static NavigationViewPages.ConfigFile appConfig;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Verifica se já existe um arquivo de configuração
            // Se sim, carrega ele
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json")))
            {
                string jsonString = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"));
                appConfig = JsonSerializer.Deserialize<NavigationViewPages.ConfigFile>(jsonString);
            }
            // Se não, cria um com configurações padrão
            else
            {
                appConfig = new ConfigFile
                {
                    AppTheme = 2,
                    DefaultDownloadsPath = $"{System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")}\\YT Downloader",
                    AlwaysAskWhereSave = true
                };
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"), JsonSerializer.Serialize(appConfig));
            }

            // Carrega a janela principal
            m_window = new MainWindow();
            m_window.Activate();
        }
    }
}
