using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;


namespace YT_Downloader.Views.Music
{
    public sealed partial class MusicPage : Page
    {
        public MusicPage()
        {
            this.InitializeComponent();
        }

        // Coleta o URL do usu�rio e envia para a pr�xima p�gina (NextVideoPage)
        private void NextButtonClicked(object sender, RoutedEventArgs e)
        {
            Views.Music.NextMusicPage.url = urlBox.Text;
            App.MainWindow.view.Navigate(typeof(Views.Music.NextMusicPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
