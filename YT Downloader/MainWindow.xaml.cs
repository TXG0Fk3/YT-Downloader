using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.ViewManagement;


namespace YT_Downloader
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            // Carrega o tema escolhido pelo usu�rio
            ApplyTheme(App.appSettings.Theme);

            // Tamanho da Janela e T�tulo "infinito"
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(660, 410));
            this.ExtendsContentIntoTitleBar = true;
            this.AppWindow.Title = "YT Downloader";
            this.AppWindow.SetIcon(@"Assets\AppIcon.ico");

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

        public void ApplyTheme(byte theme)
        {
            switch (theme)
            {
                case 0: // Claro
                    rootElement.RequestedTheme = ElementTheme.Light;
                    break;

                case 1: // Escuro
                    rootElement.RequestedTheme = ElementTheme.Dark;
                    break;

                case 2: // Padr�o do Sistema
                    rootElement.RequestedTheme = ElementTheme.Default;
                    break;
            }
        }
    }
}
