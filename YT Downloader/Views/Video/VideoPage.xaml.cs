using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;

namespace YT_Downloader.Views.Video
{
    public sealed partial class VideoPage : Page
    {
        public VideoPage()
        {
            this.InitializeComponent();
        }

        // M�todo que � chamado quando o bot�o � clicado ou Enter � pressionado
        private void NavigateToNextPage()
        {
            App.mainWindow.view.Navigate(
                typeof(Views.Video.NextVideoPage),
                urlBox.Text,
                new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight }
            );
        }

        // Manipulador de eventos para o bot�o
        private void NextButton_Clicked(object sender, RoutedEventArgs e)
        {
            NavigateToNextPage();
        }

        // Manipulador de eventos para a tecla pressionada no TextBox
        private void UrlBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                NavigateToNextPage();
            }
        }
    }
}
