using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


namespace YT_Downloader
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Carrega o tema escolhido pelo usu�rio
            ApplyTheme((ElementTheme)Enum.Parse(typeof(ElementTheme), App.appSettings.Theme));

            // Tamanho da Janela e T�tulo "infinito"
            AppWindow.Resize(new Windows.Graphics.SizeInt32(660, 410));
            ExtendsContentIntoTitleBar = true;
            AppWindow.Title = "YT Downloader";
            AppWindow.SetIcon(@"Assets\AppIcon.ico");

            // Passa o Frame (view) para todas as pages e incializa a VideoPage (Padr�o)
            view.Navigate(typeof(Views.Video.VideoPage), null); 
        }

        // M�todo que altera a page sendo mostrada
        private void NavigationViewSwitch(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            App.cts.Cancel(); // Para cancelar opera��es que podem estar sendo executadas
            if (args.IsSettingsInvoked)
            {   
                // Settings
                view.Navigate(typeof(Views.SettingsPage), null); 
            }
            else
            {
                // Views
                Type newPage = Type.GetType(args.InvokedItemContainer.Tag.ToString());
                view.Navigate(newPage, null);
            }
        }

        // Aplica o tema
        public void ApplyTheme(ElementTheme theme) { rootElement.RequestedTheme = theme; }
    }
}
