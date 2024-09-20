using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System.Diagnostics;


namespace YT_Downloader.Views
{
    public sealed partial class DownloadFinishedPage : Page
    {
        // Variáveis estáticas que serão acessadas por outras classes
        public static string downloadPath;
        public static string vidTitle;
        public static string downloadType;

        public DownloadFinishedPage()
        {
            this.InitializeComponent();
            // Mostra o título do vídeo
            videoTitle.Text = vidTitle;
        }

        // Abre o file explorer na pasta onde foi feito o download
        private void DownloadLocationButton_Clicked(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", downloadPath);
        }

        // Volta para a Page inicial
        private void BackButton_clicked(object sender, RoutedEventArgs e)
        {
            switch (downloadType)
            {
                case "V": // Vídeo
                    App.mainWindow.view.Navigate(typeof(Views.Video.VideoPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                    break;
                case "M": // Música
                    App.mainWindow.view.Navigate(typeof(Views.Music.MusicPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                    break;
                case "P": // Imagem
                    //App.mainWindow.view.Navigate(typeof(Views.Picture.PicturePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                    break;
            }
        }
    }
}
