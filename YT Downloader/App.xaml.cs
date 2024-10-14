using Microsoft.UI.Xaml;

namespace YT_Downloader
{
    public partial class App : Application
    {
        public static MainWindow mainWindow;
        public static Settings.AppSettings appSettings = new();

        public App() =>
            InitializeComponent();

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Carrega as configurações da aplicação
            appSettings.LoadSettings();

            // Carrega a janela principal
            mainWindow = new MainWindow();
            mainWindow.Activate();
        }
    }
}
