using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
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
        // Variáveis estáticas para serem acessadas por outras classes
        public static string url;

        public YoutubeClient youtube;
        public YoutubeExplode.Videos.Video video;
        public StreamManifest streamManifest;

        public NextVideoPage()
        {
            this.InitializeComponent();
            this.Loaded += NextVideoPage_Loaded;
        }

        // Método que é chamado somente quando a page estiver completamente carregada
        private void NextVideoPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.cts = new();
            GetAndShowVideoInfo(App.cts.Token);
        }

        // Coleta informações da URL ou ID do vídeo e mostra ao usuário
        async private void GetAndShowVideoInfo(CancellationToken token)
        {
            try
            {
                youtube = new YoutubeClient();

                video = await youtube.Videos.GetAsync(url);
                if (token.IsCancellationRequested) return;

                streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                if (token.IsCancellationRequested) return;

                // Título a ser mostrado pode ter no máximo 60 caracteres
                videoTitle.Text = video.Title.Length > 60 ? $"{video.Title[..60]}..." : video.Title;

                // Mostra ao Usuário todas as resolução disponíveis
                foreach (var rel in streamManifest.GetVideoOnlyStreams().Where(s => s.Container == Container.Mp4))
                {
                    if (!videoResolution.Items.Contains(rel.VideoQuality.Label))
                    {
                        videoResolution.Items.Add(new ComboBoxItem().Content = rel.VideoQuality.Label);
                    }
                }

                if (token.IsCancellationRequested) return;

                // Carrega a Thumbnail do vídeo e mostra ao usuário.
                var thumbnailUrl = $"https://img.youtube.com/vi/{video.Id}/mqdefault.jpg";
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(thumbnailUrl);
                var content = await response.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes($"{Path.GetTempPath()}\\{video.Id}.jpg", content);
                videoPicture.Source = new BitmapImage(new Uri($"{Path.GetTempPath()}\\{video.Id}.jpg"));

                loading.IsActive = false;
                loadingBorder.Visibility = Visibility.Collapsed;
                pictureBorder.Visibility = Visibility.Visible;

                await Task.Delay(40);
                File.Delete($"{Path.GetTempPath()}\\{video.Id}.jpg");

                // Habilita o botão de download
                downloadButton.IsEnabled = true;
                videoResolution.IsEnabled = true;
            }
            catch (Exception ex)
            {
                // Caso o programa tiver algum problema, uma mensagem de erro será mostrada
                ContentDialog dialog = new()
                {
                    XamlRoot = XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "An error has occurred",
                    CloseButtonText = "Close",
                    Content = new Views.ErrorPage(ex.Message)
                };

                _ = await dialog.ShowAsync();
                App.mainWindow.view.Navigate(typeof(Views.Video.VideoPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
        }

        // Caso o usuário altere a resolução, também altera o tamanho do vídeo
        private void VideoResolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Run run = new();
            run.Text = $"{Math.Round(float.Parse(streamManifest.GetVideoOnlyStreams().Where(s => s.Container == Container.Mp4).First(s => s.VideoQuality.Label == videoResolution.SelectedValue.ToString()).Size.MegaBytes.ToString().Split(" ")[0]) + float.Parse(streamManifest.GetAudioOnlyStreams().Where(s => s.Container == Container.Mp4).GetWithHighestBitrate().Size.MegaBytes.ToString().Split(" ")[0]), 2)} MB";
            videoSize.Inlines.Clear();
            videoSize.Inlines.Add(run);
        }

        // Baixa o vídeo
        async private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            // Caminho onde será baixado o vídeo
            string downloadPath = App.appSettings.DefaultDownloadsPath;
            if (App.appSettings.AlwaysAskWhereSave)
            {
                FolderPicker openPicker = new();
                openPicker.FileTypeFilter.Add("*");

                nint windowHandle = WindowNative.GetWindowHandle(App.mainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(openPicker, windowHandle);

                StorageFolder folder = await openPicker.PickSingleFolderAsync();

                if (folder != null) downloadPath = folder.Path;
                // Caso o usuário cancele a escolha da pasta
                else return;
            }

            // Envia os dados para DownloadPage.
            Views.DownloadPage.downloadPath = downloadPath;
            Views.DownloadPage.youtube = youtube;
            Views.DownloadPage.video = video;
            Views.DownloadPage.downloadType = "V";
            Views.DownloadPage.videoStreamInfo = streamManifest
                                                .GetVideoOnlyStreams()
                                                .Where(s => s.Container == Container.Mp4)
                                                .First(s => s.VideoQuality.Label == videoResolution.SelectedValue.ToString());
            Views.DownloadPage.audioStreamInfo = streamManifest
                                                .GetAudioOnlyStreams()
                                                .Where(s => s.Container == Container.Mp4)
                                                .GetWithHighestBitrate();

            App.mainWindow.view.Navigate(typeof(Views.DownloadPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        // Cancela a operação
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.cts.Cancel();
            App.mainWindow.view.Navigate(typeof(Views.Video.VideoPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
    }
}
