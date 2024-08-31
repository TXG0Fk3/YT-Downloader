using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
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
        // Variáveis estáticas acessíveis por outras classes
        public static string DownloadPath;
        public static YoutubeClient Youtube;
        public static YoutubeExplode.Videos.Video Video;
        public static string DownloadType;
        public static VideoOnlyStreamInfo VideoStreamInfo;
        public static IStreamInfo AudioStreamInfo;

        private readonly string _downloadName;

        public DownloadPage()
        {
            InitializeComponent();

            // Define título e imagem do vídeo
            videoTitle.Text = Video.Title.Length > 60 ? $"{Video.Title[..60]}..." : Video.Title;
            videoPicture.Source = new BitmapImage(new Uri($"{Path.GetTempPath()}\\{Video.Id}.jpg"));
            _downloadName = SanitizeFileName(Video.Title);

            DownloadVideo(App.cts.Token);
        }

        private static string SanitizeFileName(string fileName) =>
            Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));

        private async void DownloadVideo(CancellationToken token)
        {
            try
            {
                var startTime = DateTime.Now;

                switch (DownloadType)
                {
                    case "V":
                        await DownloadVideoFile(startTime, token);
                        break;
                    case "M":
                        await DownloadAudioFile(startTime, token);
                        break;
                }

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

            await Youtube.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder($"{DownloadPath}\\{_downloadName}.mp4").Build(),
                new Progress<double>(p => UpdateProgress(p, totalSizeMb, startTime)), token);
        }

        private async Task DownloadAudioFile(DateTime startTime, CancellationToken token)
        {
            var totalSizeMb = AudioStreamInfo.Size.Bytes / (1024 * 1024f);

            await Youtube.Videos.Streams.DownloadAsync(AudioStreamInfo, $"{DownloadPath}\\{_downloadName}.mp3",
                new Progress<double>(p => UpdateProgress(p, totalSizeMb, startTime)), token);
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

            if (DownloadType == "M") File.Delete($"{DownloadPath}\\{_downloadName}.mp3");
        }

        private void NavigateToDownloadFinishedPage()
        {
            Views.DownloadFinishedPage.downloadPath = DownloadPath;
            Views.DownloadFinishedPage.vidTitle = videoTitle.Text;
            Views.DownloadFinishedPage.downloadType = DownloadType;
            App.mainWindow.view.Navigate(typeof(Views.DownloadFinishedPage), null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void NavigateToPreviousPage()
        {
            var pageType = DownloadType == "V" ? typeof(Views.Video.VideoPage) : typeof(Views.Music.MusicPage);
            App.mainWindow.view.Navigate(pageType, null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.cts.Cancel();
            NavigateToPreviousPage();
        }
    }
}
