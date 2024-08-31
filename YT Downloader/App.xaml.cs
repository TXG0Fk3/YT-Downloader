using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace YT_Downloader
{
    public partial class App : Application
    {
        public static CancellationTokenSource cts = new();
        public static MainWindow mainWindow;
        public static Settings.AppSettings appSettings = new();

        public App()
        {
            this.InitializeComponent();
        }

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
