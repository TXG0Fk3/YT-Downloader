using Microsoft.UI.Xaml;
using System.Threading;

namespace YT_Downloader
{
    public partial class App : Application
    {
        public static CancellationTokenSource Cts = new();
        public static MainWindow MainWindow;
        public static Settings.AppSettings AppSettings = new();

        public App() => InitializeComponent();

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Carrega as configurações da aplicação
            AppSettings.LoadSettings();

            // Carrega a janela principal
            MainWindow = new MainWindow();
            MainWindow.Activate();
        }
    }
}
