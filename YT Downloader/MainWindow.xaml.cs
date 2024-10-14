using System;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;
using Windows.UI;
using Windows.UI.WindowManagement;
using WinRT.Interop;
using YT_Downloader.Views;
using YT_Downloader.Controls;
using System.IO;

namespace YT_Downloader
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            // Configurações da janela
            Activated += AdjustWindowSizeForDpi; // Altera Resolução sempre que o DPI (escala) é modificado
            Closed += MainWindow_Closed; // Método que será executado ao finalizar o programa.
            AppWindow.Title = "YT Downloader"; // Título
            AppWindow.SetIcon(@"Assets\AppIcon.ico"); // Ícone
            (AppWindow.Presenter as OverlappedPresenter).IsResizable = false; // Torna a janela não redimension�vel
            
            // Configurações da TitleBar
            ExtendsContentIntoTitleBar = true; // TitleBar infinito
            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            _ = SetWindowLong(hwnd, -16, GetWindowLong(hwnd, -16) & ~0x00010000); // Desativa o botão de maximizar

            InitializeComponent();

            // Aplica o tema
            ApplyTheme(Enum.TryParse<ElementTheme>(App.appSettings.Theme, out var parsedTheme)
                ? parsedTheme
                : ElementTheme.Default // Definindo um valor padrão, se a conversão falhar
                );
        }

        // Ajusta resolução do app de acordo com a DPI (escala) do monitor
        private void AdjustWindowSizeForDpi(object sender, WindowActivatedEventArgs args)
        {
            // Verifica se a janela está ativa
            if (args.WindowActivationState == WindowActivationState.Deactivated) return; // Se a janela não estiver ativa, não faz nada

            int dpi = GetDpiForWindow(WindowNative.GetWindowHandle(this));
            double scaleFactor = dpi / 96.0; // 96 é o DPI padrão de 100%

            // Ajusta o tamanho de acordo com a escala
            AppWindow.Resize(new SizeInt32((int)(430 * scaleFactor), (int)(680 * scaleFactor)));
        }


        // Aplica o tema
        public void ApplyTheme(ElementTheme theme)
        {
            rootElement.RequestedTheme = theme; // Aplica o tema ao rootElement (content root da janela)
            var titleBar = AppWindow.TitleBar; // Customização da TitleBar diretamente no WinUI 3

            // Determina o tema a ser aplicado
            if (theme == ElementTheme.Default)
                theme = (Application.Current.RequestedTheme == ApplicationTheme.Dark)
                    ? ElementTheme.Dark
                    : ElementTheme.Light;

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
        private Visibility CheckWithoutDownloadsCardVisibility() =>
            WithoutDownloadsCard.Visibility = DownloadsStackPanel.Children.Count > 1
            ? Visibility.Collapsed
            : Visibility.Visible;

        // Abre dialogo de Download
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

        // Abre dialogo de Ajuda
        private async void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = rootElement.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                RequestedTheme = rootElement.RequestedTheme,
                Title = "Help",
                CloseButtonText = "Close",
                Content = new HelpPage()
            };

            _ = await dialog.ShowAsync();
        }

        // Abre dialogo de Configurações
        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = rootElement.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                RequestedTheme = rootElement.RequestedTheme,
                Title = "Settings",
                CloseButtonText = "Close",
                Content = new SettingsPage()
            };

            rootElement.ActualThemeChanged += (sender, args) =>
                dialog.RequestedTheme = rootElement.RequestedTheme; // Atualiza o tema dinamicamente

            _ = await dialog.ShowAsync();
        }

        // Deleta as thumbnails baixadas e finaliza o programa
        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            if (Directory.Exists($"{Path.GetTempPath()}\\ThumbnailCache"))
                Directory.Delete($"{Path.GetTempPath()}\\ThumbnailCache", true);
        }

        [DllImport("user32.dll")]
        private static extern int GetDpiForWindow(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
