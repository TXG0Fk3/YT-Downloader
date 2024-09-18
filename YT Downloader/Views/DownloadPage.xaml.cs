using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace YT_Downloader.Views
{
    public sealed partial class DownloadPage : Page
    {
        public static string DownloadPath;
        private string FileName;
        public static YoutubeClient YoutubeClient;
        public static YoutubeExplode.Videos.Video Video;
        public static VideoOnlyStreamInfo VideoStreamInfo;
        public static IStreamInfo AudioStreamInfo;        

        public DownloadPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var parameters = e.Parameter as dynamic;
            DownloadPath = parameters.DownloadPath;
            YoutubeClient = parameters.YoutubeClient;
            Video = parameters.Video;
            VideoStreamInfo = parameters.VideoStreamInfo;
            AudioStreamInfo = parameters.AudioStreamInfo;

            // Define título e imagem do vídeo
            videoTitle.Text = Video.Title.Length > 60 ? $"{Video.Title[..60]}..." : Video.Title;
            videoPicture.Source = new BitmapImage(new Uri($"{Path.GetTempPath()}\\{Video.Id}.jpg"));
            FileName = SanitizeFileName(Video.Title);


            DownloadVideo(App.Cts.Token);
        }

        private static string SanitizeFileName(string fileName) => 
            Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));

        private async void DownloadVideo(CancellationToken token)
        {
            try
            {
                var startTime = DateTime.Now;

                await (VideoStreamInfo != null
                    ? DownloadVideoFile(startTime, token)
                    : DownloadAudioFile(startTime, token));

                NavigateToDownloadFinishedPage();
            }
            catch (Exception ex)
            {
                await HandleDownloadError(ex);
            }
        }

        private async Task DownloadVideoFile(DateTime startTime, CancellationToken token)
        {
            var streamInfos = new IStreamInfo[] { AudioStreamInfo, VideoStreamInfo };
            var totalSizeMb = streamInfos.Sum(s => s.Size.Bytes / (1024 * 1024f));

            await YoutubeClient.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder($"{DownloadPath}\\{FileName}.mp4").Build(),
                new Progress<double>(p => { if (p % 0.005 < 0.0001) { UpdateProgress(p, totalSizeMb, startTime); } }), token);
        }

        private async Task DownloadAudioFile(DateTime startTime, CancellationToken token)
        {
            var totalSizeMb = AudioStreamInfo.Size.Bytes / (1024 * 1024f);

            await YoutubeClient.Videos.Streams.DownloadAsync(AudioStreamInfo, $"{DownloadPath}\\{FileName}.mp3",
                new Progress<double>(p => { if (p % 0.005 < 0.0001) { UpdateProgress(p, totalSizeMb, startTime); } }), token);
        }

        private void UpdateProgress(double progressPercentage, float totalSizeMb, DateTime startTime)
        {
            var downloadedSizeMb = totalSizeMb * (float)progressPercentage;
            var elapsedTime = DateTime.Now - startTime;
            var downloadSpeed = downloadedSizeMb / (float)elapsedTime.TotalSeconds;

            progressBar.Value = progressPercentage * 100;
            progress.Text = $"{FormatTimeRemaining(elapsedTime, progressPercentage)} - {downloadedSizeMb:F2} MB of {totalSizeMb:F2} MB ({downloadSpeed:F2} MB/s)";
        }

        private static string FormatTimeRemaining(TimeSpan elapsedTime, double progressPercentage)
        {
            var remainingTime = TimeSpan.FromSeconds(elapsedTime.TotalSeconds / progressPercentage);
            return $"{remainingTime.Hours:D2}:{remainingTime.Minutes:D2}:{remainingTime.Seconds:D2}";
        }

        private async Task HandleDownloadError(Exception ex)
        {
            if (ex.Message != "The operation was canceled." && ex.Message != "A task was canceled.")
            {
                await Task.Delay(5); // Garante que a page seja carregada antes de mostrar a mensagem de erro

                var dialog = new ContentDialog
                {
                    XamlRoot = XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "An error has occurred",
                    CloseButtonText = "Close",
                    Content = new ErrorPage(ex.Message)
                };

                await dialog.ShowAsync();
                NavigateToPreviousPage();
            }

            if (VideoStreamInfo == null) File.Delete($"{DownloadPath}\\{FileName}.mp3");
        }

        private void NavigateToDownloadFinishedPage()
        {
            Views.DownloadFinishedPage.downloadPath = DownloadPath;
            Views.DownloadFinishedPage.vidTitle = videoTitle.Text;
            Views.DownloadFinishedPage.downloadType = VideoStreamInfo != null ? "V" : "M";
            App.MainWindow.view.Navigate(typeof(Views.DownloadFinishedPage), null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void NavigateToPreviousPage()
        {
            var pageType = VideoStreamInfo != null ? typeof(Views.Video.VideoPage) : typeof(Views.Music.MusicPage);
            App.MainWindow.view.Navigate(pageType, null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.Cts.Cancel();
            NavigateToPreviousPage();
        }
    }
}
