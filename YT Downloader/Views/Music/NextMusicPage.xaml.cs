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


namespace YT_Downloader.Views.Music
{
    public sealed partial class NextMusicPage : Page
    {
        // Vari�veis est�ticas para serem acessadas por outras classes
        public static string url;

        public YoutubeClient youtube;
        public YoutubeExplode.Videos.Video video;
        public StreamManifest streamManifest;

        public NextMusicPage()
        {
            this.InitializeComponent();
            this.Loaded += NextMusicPage_Loaded;
        }

        // M�todo que � chamado somente quando a page estiver completamente carregada
        private void NextMusicPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.cts = new CancellationTokenSource();
            GetAndShowVideoInfo(App.cts.Token);
        }

        // Coleta informa��es da URL ou ID do v�deo e mostra ao usu�rio
        async private void GetAndShowVideoInfo(CancellationToken token)
        {
            try
            {
                youtube = new YoutubeClient();

                video = await youtube.Videos.GetAsync(url);
                if (token.IsCancellationRequested) return;

                streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                if (token.IsCancellationRequested) return;

                // T�tulo a ser mostrado pode ter no m�ximo 60 caracteres
                videoTitle.Text = video.Title.Length > 60 ? $"{video.Title[..60]}..." : video.Title;

                // Mostra ao Usu�rio todas as resolu��o dispon�veis
                foreach (var rel in streamManifest.GetAudioOnlyStreams().Where(s => s.Container == Container.Mp4).OrderByDescending(s => s.Bitrate))
                {
                    if (!audioBitrate.Items.Contains($"{rel.Bitrate} {rel.AudioCodec}"))
                    {
                        audioBitrate.Items.Add(new ComboBoxItem().Content = $"{rel.Bitrate} {rel.AudioCodec}");
                    }
                }

                if (token.IsCancellationRequested) return;

                // Carrega a Thumbnail do v�deo e mostra ao usu�rio.
                var thumbnailUrl = $"https://img.youtube.com/vi/{video.Id}/mqdefault.jpg";
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(thumbnailUrl);
                var content = await response.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes($"{Path.GetTempPath()}\\{video.Id}.jpg", content);
                videoPicture.Source = new BitmapImage(new Uri($"{Path.GetTempPath()}\\{video.Id}.jpg"));

                loading.IsActive = false;
                loadingBorder.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                pictureBorder.Visibility = Microsoft.UI.Xaml.Visibility.Visible;

                await Task.Delay(40);
                File.Delete($"{Path.GetTempPath()}\\{video.Id}.jpg");

                // Habilita o bot�o de download
                downloadButton.IsEnabled = true;
                audioBitrate.IsEnabled = true;
            }
            catch (Exception ex)
            {
                // Caso o programa tiver algum problema, uma mensagem de erro ser� mostrada
                ContentDialog dialog = new()
                {
                    XamlRoot = XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "An error has occurred",
                    CloseButtonText = "Close",
                    Content = new Views.ErrorPage(ex.Message)
                };

                _ = await dialog.ShowAsync();
                //App.mainWindow.view.Navigate(typeof(Views.Music.MusicPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
        }

        // Caso o usu�rio altere o bitrate, tamb�m altera o tamanho do �udio
        private void AudioBitrate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Run run = new();
            run.Text = $"{Math.Round(float.Parse(streamManifest.GetAudioOnlyStreams().Where(s => s.Container == Container.Mp4).First(s => s.Bitrate.ToString() == $"{audioBitrate.SelectedValue.ToString().Split()[0]} {audioBitrate.SelectedValue.ToString().Split()[1]}" && s.AudioCodec == audioBitrate.SelectedValue.ToString().Split()[2]).Size.MegaBytes.ToString().Split(" ")[0]), 2)} MB";
            audioSize.Inlines.Clear();
            audioSize.Inlines.Add(run);
        }

        // Baixa o �udio
        async private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {

            // Caminho onde ser� baixado o �udio
            string downloadPath = App.appSettings.DefaultDownloadsPath;
            if (App.appSettings.AlwaysAskWhereSave)
            {
                FolderPicker openPicker = new();
                openPicker.FileTypeFilter.Add("*");

                nint windowHandle = WindowNative.GetWindowHandle(App.mainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(openPicker, windowHandle);

                StorageFolder folder = await openPicker.PickSingleFolderAsync();

                if (folder != null) downloadPath = folder.Path;
                // Caso o usu�rio cancele a escolha da pasta
                else return;
            }

            // Envia os dados para DownloadPage.
            Views.DownloadPage.DownloadPath = downloadPath;
            Views.DownloadPage.YoutubeClient = youtube;
            Views.DownloadPage.Video = video;
            Views.DownloadPage.AudioStreamInfo = streamManifest
                                                .GetAudioOnlyStreams()
                                                .Where(s => s.Container == Container.Mp4)
                                                .First(s => s.Bitrate.ToString() == $"{audioBitrate.SelectedValue.ToString().Split()[0]} {audioBitrate.SelectedValue.ToString().Split()[1]}" && s.AudioCodec == audioBitrate.SelectedValue.ToString().Split()[2]);
           
            //App.mainWindow.view.Navigate(typeof(Views.DownloadPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        // Cancela a opera��o
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.cts.Cancel();
            //App.mainWindow.view.Navigate(typeof(Views.Music.MusicPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
    }
}
