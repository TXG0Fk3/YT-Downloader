using AngleSharp.Dom;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;


namespace YT_Downloader.Views
{
    public sealed partial class DetailsPage : Page
    {
        private readonly YoutubeClient YoutubeClient;
        private YoutubeExplode.Videos.Video Video;
        private StreamManifest StreamManifest;
        private readonly string ThumbnailPath;
        private CancellationToken token;

        public DetailsPage()
        {
            YoutubeClient = new();
            InitializeComponent();
        }

        private void UrlBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                //App.mainWindow.NavigateToNextPage(typeof(Video.NextVideoPage), urlBox.Text);
            }
        }

        //private void LoadButton_Clicked(object sender, RoutedEventArgs e) =>
            //App.mainWindow.NavigateToNextPage(typeof(Video.NextVideoPage), urlBox.Text);

        private async void LoadVideoInfoAsync()
        {
            try
            {
                Video = await YoutubeClient.Videos.GetAsync(UrlTextBox.Text);
                if (token.IsCancellationRequested) return;

                StreamManifest = await YoutubeClient.Videos.Streams.GetManifestAsync(UrlTextBox.Text);
                if (token.IsCancellationRequested) return;
            }
            catch (Exception ex)
            {
                //await ShowErrorDialogAsync("An error occurred while loading the video.", ex);
                //App.mainWindow.NavigateToPreviousPage(typeof(VideoPage));
            }
        }
    }
}
