using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using YT_Downloader.ViewModels;

namespace YT_Downloader
{
    public partial class App : Application
    {
        public static MainWindow mainWindow;
        public static Settings.AppSettings appSettings = new();

        private readonly IServiceProvider _services;

        public App()
        {
            InitializeComponent();

            var services = new ServiceCollection();
            services.AddTransient<MainPageViewModel>();
            services.AddSingleton<YoutubeService>();
            services.AddSingleton<DownloadsService>();
            _services = services.BuildServiceProvider();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Carrega as configurações da aplicação
            appSettings.LoadSettings();

            // Carrega a janela principal
            mainWindow = new MainWindow();
            mainWindow.Activate();
        }

        public static T GetService<T>() where T : class => ((App)Current)._services.GetRequiredService<T>();
    }
}
