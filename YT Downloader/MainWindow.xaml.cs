using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Graphics;

namespace YT_Downloader
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Configurações da janela
            Activated += AdjustWindowSizeForDpi; // Altera Resolução sempre que o DPI (escala) é modificado
            ExtendsContentIntoTitleBar = true; // TitleBar infinito
            AppWindow.Title = "YT Downloader"; // Título
            AppWindow.SetIcon(@"Assets\AppIcon.ico"); // Ícone

            // Aplica o tema
            ApplyTheme(Enum.TryParse<ElementTheme>(App.AppSettings.Theme, out var parsedTheme)
                ? parsedTheme
                : ElementTheme.Default // Definindo um valor padrão, se a conversão falhar
                );

            // Página inicial
            view.Navigate(typeof(Views.Video.VideoPage));
        }

        // Ajusta resolução do app de acordo com a DPI (escala) do monitor
        private void AdjustWindowSizeForDpi(object sender, WindowActivatedEventArgs args)
        {
            int dpi = GetDpiForWindow(WinRT.Interop.WindowNative.GetWindowHandle(this));
            double scaleFactor = dpi / 96.0; // 96 é o DPI padrão de 100%

            // Ajusta de acordo com a escala
            double Width = 660 * scaleFactor;
            double Height = 410 * scaleFactor;
            AppWindow.Resize(new SizeInt32((int)Width, (int)Height));
        }

        // Aplica o tema
        public void ApplyTheme(ElementTheme theme) =>
            rootElement.RequestedTheme = theme;

        // Método que altera a page sendo mostrada
        private void NavigationViewSwitch(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            App.Cts.Cancel(); // Cancela operações que podem estar em andamento

            if (args.IsSettingsInvoked)
            {
                // Navega para a SettingsPage
                view.Navigate(typeof(Views.SettingsPage));
            }
            else
            {
                // Navega para a página especificada pelo usuário
                Type newPage = Type.GetType(args.InvokedItemContainer.Tag.ToString());
                view.Navigate(newPage, null);
            }
        }

        // Método que altera o view (Frame) com uma animação indo para direita (indo).
        public void NavigateToNextPage<TParameter>(Type nextPage, TParameter parameter) =>
            view.Navigate(nextPage, parameter, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });

        // Método que altera o view (Frame) com uma animação indo para esquerda (voltando).
        public void NavigateToPreviousPage(Type previousPage) =>
            view.Navigate(previousPage, null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });

        [DllImport("user32.dll")]
        private static extern int GetDpiForWindow(IntPtr hwnd);
    }
}
