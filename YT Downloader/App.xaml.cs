using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Graphics;
using YT_Downloader.Enums;
using YT_Downloader.Helpers.UI;
using YT_Downloader.Messages;
using YT_Downloader.Services;
using YT_Downloader.ViewModels;
using YT_Downloader.ViewModels.Dialogs;
using YT_Downloader.Views;

namespace YT_Downloader
{
    public partial class App : Application,
        IRecipient<ChangeThemeRequestMessage>
    {
        private Window _mainWindow;

        private readonly IServiceProvider _services;
        private readonly IMessenger _messenger = WeakReferenceMessenger.Default;

        public App()
        {
            InitializeComponent();
            _messenger.RegisterAll(this);

            var services = new ServiceCollection();
            services.AddTransient<MainPageViewModel>();
            services.AddTransient<DetailsDialogViewModel>();

            services.AddSingleton<YoutubeService>();
            services.AddSingleton<DownloadsService>();
            services.AddSingleton<SettingsService>();
            services.AddSingleton<DialogService>();
            services.AddSingleton(_messenger);
            _services = services.BuildServiceProvider();
        }

        public static T GetService<T>() where T : class => ((App)Current)._services.GetRequiredService<T>();

        public void Receive(ChangeThemeRequestMessage message) =>
            ApplyTheme(message.Theme);

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _mainWindow = new Window
            {
                SystemBackdrop = new MicaBackdrop(),
                ExtendsContentIntoTitleBar = true,
                Title = "YT Downloader",
                Content = new MainPage()
            };

            _mainWindow.AppWindow.SetIcon("Assets/AppIcon.ico");

            var win32WindowService = new Win32WindowService(_mainWindow);
            win32WindowService.SetWindowMinMaxSize(new Win32WindowService.POINT() { x = 430, y = 680 });

            var scaleFactor = win32WindowService.GetSystemDPI() / 96.0;
            _mainWindow.AppWindow.Resize(new SizeInt32((int)(430 * scaleFactor), (int)(680 * scaleFactor)));

            ApplyTheme(GetService<SettingsService>().Current.Theme);

            _mainWindow.Activate();
        }

        private void ApplyTheme(ThemeOption theme) =>
            ThemeHelper.ApplyTheme(_mainWindow, ThemeHelper.ConvertThemeOptionToElementTheme(theme));
    }
}
