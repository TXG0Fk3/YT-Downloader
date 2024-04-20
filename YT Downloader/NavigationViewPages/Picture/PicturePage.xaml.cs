using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;


namespace YT_Downloader.NavigationViewPages.Picture
{
    public sealed partial class PicturePage : Page
    {
        public static Frame view;

        public PicturePage()
        {
            this.InitializeComponent();
        }

        // Coleta o URL do usuário e envia para a próxima página (NextVideoPage)
        private void NextButtonClicked(object sender, RoutedEventArgs e)
        {
            NavigationViewPages.Picture.NextPicturePage.url = urlBox.Text;
            NavigationViewPages.Picture.NextPicturePage.view = view;
            view.Navigate(typeof(NavigationViewPages.Picture.NextPicturePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
