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

        // Manipulador de eventos para o botão
        //private void NextButton_Clicked(object sender, RoutedEventArgs e) =>
            //App.mainWindow.NavigateToNextPage(typeof(Video.NextVideoPage), urlBox.Text);

        // Manipulador de eventos para a tecla pressionada no TextBox
        private void UrlBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                //App.mainWindow.NavigateToNextPage(typeof(Video.NextVideoPage), urlBox.Text);
            }
        }
    }
}
