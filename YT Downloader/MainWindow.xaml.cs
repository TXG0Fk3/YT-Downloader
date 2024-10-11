using System;
using System.Runtime.InteropServices;
using AngleSharp.Dom;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;
using Windows.UI;
using Windows.UI.WindowManagement;
using WinRT.Interop;
using YoutubeExplode.Videos;
using YoutubeExplode;
using YT_Downloader.Views;
using YoutubeExplode.Videos.Streams;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using CommunityToolkit.WinUI;
using YT_Downloader.Controls;

namespace YT_Downloader
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            // Configura��es da janela
            Activated += AdjustWindowSizeForDpi; // Altera Resolu��o sempre que o DPI (escala) � modificado
            AppWindow.Title = "YT Downloader"; // T�tulo
            AppWindow.SetIcon(@"Assets\AppIcon.ico"); // �cone
            (AppWindow.Presenter as OverlappedPresenter).IsResizable = false; // Torna a janela n�o redimension�vel
            
            // Configura��es da TitleBar
            ExtendsContentIntoTitleBar = true; // TitleBar infinito
            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            _ = SetWindowLong(hwnd, -16, GetWindowLong(hwnd, -16) & ~0x00010000); // Desativa o bot�o de maximizar

            InitializeComponent();

            // Aplica o tema
            ApplyTheme(Enum.TryParse<ElementTheme>(App.appSettings.Theme, out var parsedTheme)
                ? parsedTheme
                : ElementTheme.Default // Definindo um valor padr�o, se a convers�o falhar
                );
        }

        // Ajusta resolu��o do app de acordo com a DPI (escala) do monitor
        private void AdjustWindowSizeForDpi(object sender, WindowActivatedEventArgs args)
        {
            int dpi = GetDpiForWindow(WindowNative.GetWindowHandle(this));
            double scaleFactor = dpi / 96.0; // 96 � o DPI padr�o de 100%

            // Ajusta de acordo com a escala    
            AppWindow.Resize(new SizeInt32((int)(430 * scaleFactor), (int)(680 * scaleFactor)));
        }

        // Aplica o tema
        public void ApplyTheme(ElementTheme theme)
        {
            rootElement.RequestedTheme = theme; // Aplica o tema ao rootElement (content root da janela)
            var titleBar = AppWindow.TitleBar; // Customiza��o da TitleBar diretamente no WinUI 3

            // Determina o tema a ser aplicado
            if (theme == ElementTheme.Default)
            {
                theme = (Application.Current.RequestedTheme == ApplicationTheme.Dark)
                    ? ElementTheme.Dark
                    : ElementTheme.Light;
            }

            Color buttonHoverBackgroundColor = theme == ElementTheme.Dark ? Color.FromArgb(255, 61, 61, 61) : Colors.LightGray;
            Color foregroundColor = theme == ElementTheme.Dark ? Colors.White : Colors.Black;

            titleBar.ButtonHoverBackgroundColor = buttonHoverBackgroundColor;
            titleBar.ForegroundColor = foregroundColor;
            titleBar.ButtonForegroundColor = foregroundColor;
            titleBar.ButtonHoverForegroundColor = foregroundColor;
        }

        public void AddDownloadToStack(DownloadCard downloadCard)
        {
            DownloadsStackPanel.Children.Add(downloadCard);
            CheckWithoutDownloadsCardVisibility();
        }
            
        public void RemoveDownloadFromStack(DownloadCard downloadCard)
        {
            DownloadsStackPanel.Children.Remove(downloadCard);
            CheckWithoutDownloadsCardVisibility();
        }

        // Checa se há itens em DownloadsStackPanel e altera sua visibilidade
        private Microsoft.UI.Xaml.Visibility CheckWithoutDownloadsCardVisibility() =>
            WithoutDownloadsCard.Visibility = DownloadsStackPanel.Children.Count != 0
            ? Microsoft.UI.Xaml.Visibility.Collapsed
            : Microsoft.UI.Xaml.Visibility.Visible;

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = rootElement.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                RequestedTheme = rootElement.RequestedTheme,
                Title = "Add New Download",
                PrimaryButtonText = "Download",
                IsPrimaryButtonEnabled = false,
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            dialog.Content = new DetailsPage(dialog);

            _ = await dialog.ShowAsync();
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = rootElement.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                RequestedTheme = rootElement.RequestedTheme, // TO-DO: respeitar o tema escolhido pelo usu�rio
                Title = "Settings",
                CloseButtonText = "Close",
                Content = new SettingsPage()
            };

            rootElement.ActualThemeChanged += (sender, args) =>
                dialog.RequestedTheme = rootElement.RequestedTheme; // Atualiza o tema dinamicamente

            _ = await dialog.ShowAsync();
        }

        [DllImport("user32.dll")]
        private static extern int GetDpiForWindow(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
