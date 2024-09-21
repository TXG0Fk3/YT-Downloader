using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Graphics;
using Windows.UI;
using Windows.UI.WindowManagement;
using WinRT.Interop;
using YT_Downloader.Views;

namespace YT_Downloader
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Configurações da janela
            Activated += AdjustWindowSizeForDpi; // Altera Resolução sempre que o DPI (escala) é modificado
            AppWindow.Title = "YT Downloader"; // Título
            AppWindow.SetIcon(@"Assets\AppIcon.ico"); // Ícone
            (AppWindow.Presenter as OverlappedPresenter).IsResizable = false; // Torna a janela não redimensionável

            // Configurações da TitleBar
            ExtendsContentIntoTitleBar = true; // TitleBar infinito
            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            _ = SetWindowLong(hwnd, -16, GetWindowLong(hwnd, -16) & ~0x00010000); // Desativa o botão de maximizar

            // Aplica o tema
            ApplyTheme(Enum.TryParse<ElementTheme>(App.appSettings.Theme, out var parsedTheme)
                ? parsedTheme
                : ElementTheme.Default // Definindo um valor padrão, se a conversão falhar
                );
        }

        // Ajusta resolução do app de acordo com a DPI (escala) do monitor
        private void AdjustWindowSizeForDpi(object sender, WindowActivatedEventArgs args)
        {
            int dpi = GetDpiForWindow(WindowNative.GetWindowHandle(this));
            double scaleFactor = dpi / 96.0; // 96 é o DPI padrão de 100%

            // Ajusta de acordo com a escala    
            AppWindow.Resize(new SizeInt32((int)(470 * scaleFactor), (int)(700 * scaleFactor)));
        }

        // Aplica o tema
        public void ApplyTheme(ElementTheme theme)
        {
            rootElement.RequestedTheme = theme; // Aplica o tema ao rootElement (content root da janela)
            var titleBar = AppWindow.TitleBar; // Customização da TitleBar diretamente no WinUI 3

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

        private async void ShowDialog_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = rootElement.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                //RequestedTheme = ElementTheme.Default, // TO-DO: respeitar o tema escolhido pelo usuário
                Title = "Settings",
                // CloseButtonText = "Close",
                Content = new SettingsPage()
            };

            _ = await dialog.ShowAsync();
        }

        private void NavigationViewSwitch(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
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

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
