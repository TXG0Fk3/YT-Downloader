using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

namespace YT_Downloader
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Configura��es da janela
            Activated += AdjustWindowSizeForDpi; // Altera Resolu��o sempre que o DPI (escala) � modificado
            ExtendsContentIntoTitleBar = true; // TitleBar infinito
            AppWindow.Title = "YT Downloader"; // T�tulo
            AppWindow.SetIcon(@"Assets\AppIcon.ico"); // �cone

            // Aplica o tema
            ApplyTheme(Enum.TryParse<ElementTheme>(App.appSettings.Theme, out var parsedTheme)
                ? parsedTheme
                : ElementTheme.Default // Definindo um valor padr�o, se a convers�o falhar
                );

            // P�gina inicial
            view.Navigate(typeof(Views.Video.VideoPage));
        }

        // Ajusta resolu��o do app de acordo com a DPI (escala) do monitor
        private void AdjustWindowSizeForDpi(object sender, WindowActivatedEventArgs args)
        {
            int dpi = GetDpiForWindow(WinRT.Interop.WindowNative.GetWindowHandle(this));
            double scaleFactor = dpi / 96.0; // 96 � o DPI padr�o de 100%

            // Ajusta de acordo com a escala
            double Width = 660 * scaleFactor;
            double Height = 410 * scaleFactor;
            AppWindow.Resize(new SizeInt32((int)Width, (int)Height));
        }

        // Aplica o tema
        public void ApplyTheme(ElementTheme theme)
        {
            rootElement.RequestedTheme = theme;
        }

        // M�todo que altera a page sendo mostrada
        private void NavigationViewSwitch(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            App.cts.Cancel(); // Cancela opera��es que podem estar em andamento

            if (args.IsSettingsInvoked)
            {
                // Navega para a SettingsPage
                view.Navigate(typeof(Views.SettingsPage));
            }
            else
            {
                // Navega para a p�gina especificada pelo usu�rio
                Type newPage = Type.GetType(args.InvokedItemContainer.Tag.ToString());
                view.Navigate(newPage, null);
            }
        }

        [DllImport("user32.dll")]
        private static extern int GetDpiForWindow(IntPtr hwnd);
    }
}
