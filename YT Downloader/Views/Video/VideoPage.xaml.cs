using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;


namespace YT_Downloader.Views.Video
{
    public sealed partial class VideoPage : Page
    {
        public VideoPage()
        {
            this.InitializeComponent();
        }

        // Coleta o URL do usuário e envia para a próxima página (NextVideoPage)
        private void NextButtonClicked(object sender, RoutedEventArgs e)
        {
            Views.Video.NextVideoPage.url = urlBox.Text;
            App.mainWindow.view.Navigate(typeof(Views.Video.NextVideoPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
