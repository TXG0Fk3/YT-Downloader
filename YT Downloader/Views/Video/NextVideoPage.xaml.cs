using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YT_Downloader.Views.Video
{
    public sealed partial class NextVideoPage : Page
    {
        private string Url;
        private YoutubeClient YoutubeClient;
        private YoutubeExplode.Videos.Video Video;
        private StreamManifest StreamManifest;

        public NextVideoPage()
        {
            InitializeComponent();
            Loaded += OnPageLoaded;
        }

        // M�todo chamado sempre que a navega��o para esta p�gina ocorre
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Url = e.Parameter as string;
        }

        // M�todo chamado quando a p�gina � carregada
        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            App.cts = new CancellationTokenSource();
            await LoadVideoInfoAsync(App.cts.Token);
        }

        // Coleta informa��es da URL ou ID do v�deo e mostra ao usu�rio
        private async Task LoadVideoInfoAsync(CancellationToken token)
        {
            try
            {
                YoutubeClient = new YoutubeClient();
                Video = await YoutubeClient.Videos.GetAsync(Url);
                if (token.IsCancellationRequested) return;

                StreamManifest = await YoutubeClient.Videos.Streams.GetManifestAsync(Url);
                if (token.IsCancellationRequested) return;

                DisplayVideoInfo();
                EnableDownloadButton();
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync("An error occurred while loading the video.", ex);
                 //App.mainWindow.NavigateToPreviousPage(typeof(VideoPage));
            }
        }

        // Exibe as informa��es do v�deo na UI
        private void DisplayVideoInfo()
        {
            videoTitle.Text = Video.Title.Length > 60 ? $"{Video.Title[..60]}..." : Video.Title;
            videoResolution.Items.Clear();

            foreach (var rel in StreamManifest.GetVideoOnlyStreams().Where(s => s.Container == Container.Mp4))
            {
                if (!videoResolution.Items.Contains(rel.VideoQuality.Label))
                {
                    videoResolution.Items.Add(new ComboBoxItem().Content = rel.VideoQuality.Label);
                }
            }

            LoadVideoThumbnail();
        }

        // Carrega e exibe a miniatura do v�deo
        private async void LoadVideoThumbnail()
        {
            string thumbnailUrl = $"https://img.youtube.com/vi/{Video.Id}/mqdefault.jpg";
            string tempFilePath = $"{Path.GetTempPath()}\\{Video.Id}.jpg";

            using (var httpClient = new HttpClient())
            {
                var content = await httpClient.GetByteArrayAsync(thumbnailUrl);
                await File.WriteAllBytesAsync(tempFilePath, content);
            }

            pictureBorder.Child = new Image
            {
                Source = new BitmapImage(new Uri(tempFilePath)),
                Width = 460,
                Height = 260,
                Stretch = Microsoft.UI.Xaml.Media.Stretch.Fill
            };

            await Task.Delay(30);
            File.Delete(tempFilePath);
        }

        // Habilita o bot�o de download
        private void EnableDownloadButton()
        {
            downloadButton.IsEnabled = true;
            videoResolution.IsEnabled = true;
        }

        // Atualiza o tamanho do v�deo baseado na resolu��o escolhida
        private void VideoResolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedStream = StreamManifest
                .GetVideoOnlyStreams()
                .Where(s => s.Container == Container.Mp4)
                .First(s => s.VideoQuality.Label == videoResolution.SelectedValue.ToString());

            var audioStream = StreamManifest.GetAudioOnlyStreams()
                .Where(s => s.Container == Container.Mp4)
                .GetWithHighestBitrate();

            double totalSize = selectedStream.Size.MegaBytes + audioStream.Size.MegaBytes;
            videoSize.Inlines.Clear();
            videoSize.Inlines.Add(new Run { Text = $"{Math.Round(totalSize, 2)} MB" });
        }

        // Inicia o download do v�deo
        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string downloadPath = await GetDownloadPathAsync();
            if (string.IsNullOrEmpty(downloadPath)) return;

            var parameters = new
            {
                DownloadPath = downloadPath,
                YoutubeClient,
                Video,
                VideoStreamInfo = StreamManifest.GetVideoOnlyStreams()
                    .Where(s => s.Container == Container.Mp4)
                    .First(s => s.VideoQuality.Label == videoResolution.SelectedValue.ToString()),
                AudioStreamInfo = StreamManifest.GetAudioOnlyStreams()
                    .Where(s => s.Container == Container.Mp4)
                    .GetWithHighestBitrate()
            };

            //App.mainWindow.NavigateToNextPage(typeof(DownloadPage), parameters);
        }

        // Obt�m o caminho de download escolhido pelo usu�rio
        private async Task<string> GetDownloadPathAsync()
        {
            if (App.appSettings.AlwaysAskWhereSave)
            {
                var openPicker = new FolderPicker
                {
                    FileTypeFilter = { "*" }
                };

                InitializeWithWindow.Initialize(openPicker, WindowNative.GetWindowHandle(App.mainWindow));

                StorageFolder folder = await openPicker.PickSingleFolderAsync();
                return folder?.Path;
            }

            return App.appSettings.DefaultDownloadsPath;
        }

        // Exibe um di�logo de erro
        private async Task ShowErrorDialogAsync(string title, Exception ex)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = title,
                Content = new Views.ErrorPage(ex.Message),
                CloseButtonText = "Close"
            };

            await dialog.ShowAsync();
        }

        // Cancela a opera��o de carregamento ou download
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.cts.Cancel();
            //App.mainWindow.NavigateToPreviousPage(typeof(VideoPage));
        }
    }
}
