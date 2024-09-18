using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;


namespace YT_Downloader.Views.Picture
{
    public sealed partial class PicturePage : Page
    {
        public PicturePage()
        {
            this.InitializeComponent();
        }

        // Coleta o URL do usuário e envia para a próxima página (NextVideoPage)
        private void NextButtonClicked(object sender, RoutedEventArgs e)
        {
            NextPicturePage.url = urlBox.Text;
            App.MainWindow.view.Navigate(typeof(Views.Picture.NextPicturePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
