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


namespace YT_Downloader.NavigationViewPages
{
    public sealed partial class DownloadPage : Page
    {
        // Variávies estáticas que serão acessadas por outras classes
        public static Frame view;
        public static string downloadPath;
        public static YoutubeClient youtube;
        public static YoutubeExplode.Videos.Video video;
        public static string downloadType;
        public static VideoOnlyStreamInfo videoStreamInfo;
        public static IStreamInfo audioStreamInfo;

        private string downloadName;

        public DownloadPage()
        {
            this.InitializeComponent();

            // Mostra o nome do vídeo e a imagem.
            videoTitle.Text = video.Title.Length > 60 ? $"{video.Title[..60]}..." : video.Title;
            videoPicture.Source = new BitmapImage(new Uri($"{Path.GetTempPath()}\\{video.Id}.jpg"));
            downloadName = video.Title.Replace("\\", "").Replace("<", "").Replace(">", "").Replace(":", "").Replace("*", "").Replace("?", "").Replace("\"", "").Replace("/", "").Replace("|", "");

            DownloadVideo(App.cts.Token);
        }

        async void DownloadVideo(CancellationToken token)
        {
            try
            {
                // Declarando variáveis que guardará status do download
                float totalSize, totalSizeMb;
                DateTime startTime = DateTime.Now;

                // Decide qual será o tipo de download
                switch (downloadType)
                {
                    case "V": // Video
                        // Faz o Download
                        // Carrega os dados para inciar o download
                        var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };

                        // dados para exibir ao usuário
                        totalSize = streamInfos.Sum(s => float.Parse(s.Size.Bytes.ToString().Split(" ")[0]));
                        totalSizeMb = totalSize / (1024 * 1024);

                        await youtube.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder($"{downloadPath}\\{downloadName}.mp4").Build(), new Progress<double>(p =>
                        {
                            // Mostra o progresso do Download
                            var downloadedSize = totalSize * p;
                            var downloadedSizeMb = downloadedSize / (1024 * 1024);
                            var elapsedTime = DateTime.Now - startTime;
                            var remainingTimeInSeconds = elapsedTime.TotalSeconds / p;
                            var hours = (int)remainingTimeInSeconds / 3600;
                            var minutes = ((int)remainingTimeInSeconds % 3600) / 60;
                            var seconds = (int)remainingTimeInSeconds % 60;
                            var formatted_remaining_time = $"{hours}:{minutes}:{seconds}";

                            var downloadSpeed = downloadedSize / elapsedTime.TotalSeconds;

                            progressBar.Value = p * 100;
                            progress.Text = $"{formatted_remaining_time} - {downloadedSizeMb:F2} MB of {totalSizeMb:F2} MB ({downloadSpeed / (1024 * 1024):F2} MB/s)";
                        }), App.cts.Token);
                        break;

                    case "M": // Music
                        // dados para exibir ao usuário
                        totalSize = float.Parse(audioStreamInfo.Size.Bytes.ToString().Split()[0]);
                        totalSizeMb = totalSize / (1024 * 1024);

                        await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, $"{downloadPath}\\{downloadName}.mp3", new Progress<double>(p =>
                        {
                            // Mostra o progresso do Download
                            var downloadedSize = totalSize * p;
                            var downloadedSizeMb = downloadedSize / (1024 * 1024);
                            var elapsedTime = DateTime.Now - startTime;
                            var remainingTimeInSeconds = elapsedTime.TotalSeconds / p;
                            var hours = (int)remainingTimeInSeconds / 3600;
                            var minutes = ((int)remainingTimeInSeconds % 3600) / 60;
                            var seconds = (int)remainingTimeInSeconds % 60;
                            var formatted_remaining_time = $"{hours}:{minutes}:{seconds}";

                            var downloadSpeed = downloadedSize / elapsedTime.TotalSeconds;

                            progressBar.Value = p * 100;
                            progress.Text = $"{formatted_remaining_time} - {downloadedSizeMb:F2} MB of {totalSizeMb:F2} MB ({downloadSpeed / (1024 * 1024):F2} MB/s)";
                        }), App.cts.Token);
                        break;

                    case "P": // Picture
                        break;
                }

                // Envia informações para o DownloadFinishedPage e inicializa ele
                NavigationViewPages.DownloadFinishedPage.view = view;
                NavigationViewPages.DownloadFinishedPage.downloadPath = downloadPath;
                NavigationViewPages.DownloadFinishedPage.vidTitle = video.Title;
                view.Navigate(typeof(NavigationViewPages.DownloadFinishedPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
            catch (Exception ex)
            {
                if (ex.Message != "The operation was canceled." && ex.Message != "A task was canceled.")
                {
                    // Para que a page seja carregada antes de tentar mostrar a mensagem de erro
                    await Task.Delay(5);

                    // Mostra para o usuário uma mensagem de erro caso algo de errado aconteça
                    ContentDialog dialog = new()
                    {
                        XamlRoot = this.XamlRoot,
                        Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                        Title = "An error has occurred",
                        CloseButtonText = "Close",
                        Content = new ErrorPage("This type of error can hardly happen, but we are trying to correct it. Try downloading the video with another resolution.\n" + ex.Message)
                    };

                    var result = await dialog.ShowAsync();
                    view.Navigate(typeof(NavigationViewPages.Video.VideoPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                }

                if (downloadType == "M") File.Delete($"{downloadPath}\\{downloadName}.mp3");
            }
        }

        // Cancela a operação
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.cts.Cancel();
            view.Navigate(typeof(NavigationViewPages.Video.VideoPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
    }
}
