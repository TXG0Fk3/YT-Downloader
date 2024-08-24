using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;


namespace YT_Downloader.Views.Picture
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
            Views.Picture.NextPicturePage.url = urlBox.Text;
            Views.Picture.NextPicturePage.view = view;
            view.Navigate(typeof(Views.Picture.NextPicturePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
