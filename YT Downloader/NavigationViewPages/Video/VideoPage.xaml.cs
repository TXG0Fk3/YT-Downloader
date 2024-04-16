using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;


namespace YT_Downloader.NavigationViewPages.Video
{
    public sealed partial class VideoPage : Page
    {
        public static Frame view;

        public VideoPage()
        {
            this.InitializeComponent();
        }

        // Coleta o URL do usuário e envia para a próxima página (NextVideoPage)
        private void NextButtonClicked(object sender, RoutedEventArgs e)
        {
            NavigationViewPages.Video.NextVideoPage.url = urlbox.Text;
            NavigationViewPages.Video.NextVideoPage.view = view;
            view.Navigate(typeof(NavigationViewPages.Video.NextVideoPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
