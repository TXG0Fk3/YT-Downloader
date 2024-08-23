using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


namespace YT_Downloader
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            // Carrega o tema escolhido pelo usu�rio
            switch (App.appConfig.AppTheme)
            {
                case 0:
                    rootElement.RequestedTheme = ElementTheme.Light; // Claro
                    break;

                case 1:
                    rootElement.RequestedTheme = ElementTheme.Dark; // Escuro
                    break;

                case 2:
                    rootElement.RequestedTheme = ElementTheme.Default; // Padr�o do Sistema
                    break;
            }

            // Tamanho da Janela e T�tulo "infinito"
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(660, 410));
            this.ExtendsContentIntoTitleBar = true;
            this.AppWindow.Title = "YT Downloader";
            this.AppWindow.SetIcon(@"Assets\AppIcon.ico");

            // Passa o Frame (view) para todas as pages e incializa a VideoPage (Padr�o)
            NavigationViewPages.Video.VideoPage.view = view;
            NavigationViewPages.Music.MusicPage.view = view;
            NavigationViewPages.Picture.PicturePage.view = view;
            NavigationViewPages.SettingsPage.rootElement = rootElement;
            view.Navigate(typeof(NavigationViewPages.Video.VideoPage), null);
        }

        // M�todo que altera a page sendo mostrada
        public void NavigationViewSwitch(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            App.cts.Cancel(); // Para cancelar opera��es que podem estar sendo executadas
            if (args.IsSettingsInvoked)
            {   // Settings
                view.Navigate(typeof(NavigationViewPages.SettingsPage), null); 
            }
            else
            {
                var tag = args.InvokedItemContainer.Tag.ToString();
                switch (tag)
                {
                    case "vid": // Video
                        view.Navigate(typeof(NavigationViewPages.Video.VideoPage), null);
                        break;

                    case "mus": // Music
                        view.Navigate(typeof(NavigationViewPages.Music.MusicPage), null);
                        break;

                    case "pic": // Picture
                        view.Navigate(typeof(NavigationViewPages.Picture.PicturePage), null);
                        break;
                }
            }
        }
    }
}
