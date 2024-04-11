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
using Windows.Storage.Pickers;
using Windows.Storage;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;


namespace YT_Downloader.NavigationViewPages.Video
{
    public sealed partial class NextVideoPage : Page
    {
        public static Frame view;
        public static string url;
        public YoutubeClient youtube;
        public YoutubeExplode.Videos.Video video;
        public StreamManifest streamManifest;

        public NextVideoPage()
        {
            this.InitializeComponent();
            this.Loaded += NextVideoPage_Loaded;
        }

        private void NextVideoPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.cts = new CancellationTokenSource();
            GetAndShowVideoInfo(App.cts.Token);
        }

        async private void GetAndShowVideoInfo(CancellationToken token)
        {
            try
            {
                youtube = new YoutubeClient();
                video = await youtube.Videos.GetAsync(url);
                if (token.IsCancellationRequested) return;
                streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                if (token.IsCancellationRequested) return;
                videoTitle.Text = video.Title.Length > 68 ? $"{video.Title[..68]}..." : video.Title;

                foreach (var rel in streamManifest.GetVideoOnlyStreams().Where(s => s.Container == Container.Mp4))
                {
                    if (!videoResolution.Items.Contains(rel.VideoQuality.Label))
                    {
                        videoResolution.Items.Add(new ComboBoxItem().Content = rel.VideoQuality.Label);
                    }
                }

                if (token.IsCancellationRequested) return;

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
                downloadButton.IsEnabled = true;  
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new()
                {
                    XamlRoot = XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "An error has occurred",
                    CloseButtonText = "Close",
                    Content = new NavigationViewPages.ErrorPage(ex.Message)
                };

                var result = await dialog.ShowAsync();
                view.Navigate(typeof(NavigationViewPages.Video.VideoPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
        }

        private void VideoResolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Run run = new();
            run.Text = $"{Math.Round(float.Parse(streamManifest.GetVideoOnlyStreams().Where(s => s.Container == Container.Mp4).First(s => s.VideoQuality.Label == videoResolution.SelectedValue.ToString()).Size.MegaBytes.ToString().Split(" ")[0]) + float.Parse(streamManifest.GetAudioOnlyStreams().Where(s => s.Container == Container.Mp4).GetWithHighestBitrate().Size.MegaBytes.ToString().Split(" ")[0]), 2)} MB";
            videoSize.Inlines.Clear();
            videoSize.Inlines.Add(run);
        }

        async private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string downloadPath = App.appConfig.DefaultDownloadsPath;
            if (App.appConfig.AlwaysAskWhereSave)
            {
                FolderPicker openPicker = new();
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.m_window);
                WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

                openPicker.SuggestedStartLocation = PickerLocationId.Desktop;

                StorageFolder folder = await openPicker.PickSingleFolderAsync();

                if (folder != null) downloadPath = folder.Path;
                else return;
            }

            NavigationViewPages.DownloadPage.view = view;
            NavigationViewPages.DownloadPage.downloadPath = downloadPath;
            NavigationViewPages.DownloadPage.youtube = youtube;
            NavigationViewPages.DownloadPage.video = video;
            NavigationViewPages.DownloadPage.downloadType = "V";
            NavigationViewPages.DownloadPage.videoStreamInfo = streamManifest
                                                .GetVideoOnlyStreams()
                                                .Where(s => s.Container == Container.Mp4)
                                                .First(s => s.VideoQuality.Label == videoResolution.SelectedValue.ToString());
            NavigationViewPages.DownloadPage.audioStreamInfo = streamManifest
                                                .GetAudioOnlyStreams()
                                                .Where(s => s.Container == Container.Mp4 && s.AudioCodec.ToString() == "mp4a.40.2")
                                                .GetWithHighestBitrate();
            view.Navigate(typeof(NavigationViewPages.DownloadPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.cts.Cancel();
            view.Navigate(typeof(NavigationViewPages.Video.VideoPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
    }
}
