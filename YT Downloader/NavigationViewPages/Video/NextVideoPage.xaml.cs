using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;


namespace YT_Downloader.NavigationViewPages.Video
{
    public sealed partial class NextVideoPage : Page
    {
        public static Frame view;
        public static string url;
        public static YoutubeExplode.Videos.Video video;
        public static StreamManifest streamManifest;

        public NextVideoPage()
        {
            this.InitializeComponent();
            this.Loaded += NextVideoPage_Loaded;
        }

        private void NextVideoPage_Loaded(object sender, RoutedEventArgs e)
        {
            GetAndShowVideoInfo();
        }

        async private void GetAndShowVideoInfo()
        {
            try
            {
                var youtube = new YoutubeClient();
                video = await youtube.Videos.GetAsync(url);
                streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                videoTitle.Text = video.Title.Length > 68 ? $"{video.Title[..68]}..." : video.Title;

                foreach (var rel in streamManifest.GetVideoOnlyStreams().Where(s => s.Container == Container.Mp4))
                {
                    if (!videoResolution.Items.Contains(rel.VideoQuality.Label))
                    {
                        videoResolution.Items.Add(new ComboBoxItem().Content = rel.VideoQuality.Label);
                    }
                }

                var thumbnailUrl = $"https://img.youtube.com/vi/{video.Id}/mqdefault.jpg";
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(thumbnailUrl);
                var content = await response.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes($"{Path.GetTempPath()}\\tempvideothumbmqres.jpg", content);
                videoPicture.Source = new BitmapImage(new Uri($"{Path.GetTempPath()}\\tempvideothumbmqres.jpg"));

                loading.IsActive = false;
                loadingBorder.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                pictureBorder.Visibility = Microsoft.UI.Xaml.Visibility.Visible;

                await Task.Delay(50);
                File.Delete($"{Path.GetTempPath()}\\tempvideothumbmqres.jpg");
                downloadButton.IsEnabled = true;  
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new()
                {
                    XamlRoot = this.XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "An error has occurred",
                    CloseButtonText = "Close",
                    Content = new NavigationViewPages.ErrorPage(ex.Message)
                };

                var result = await dialog.ShowAsync();
                view.Navigate(typeof(NavigationViewPages.Video.VideoPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
        }

        private void videoResolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Run run = new();
            run.Text = $"{Math.Round(float.Parse(streamManifest.GetVideoOnlyStreams().Where(s => s.Container == Container.Mp4).First(s => s.VideoQuality.Label == videoResolution.SelectedValue.ToString()).Size.MegaBytes.ToString().Split(" ")[0]) + float.Parse(streamManifest.GetAudioOnlyStreams().Where(s => s.Container == Container.Mp4).GetWithHighestBitrate().Size.MegaBytes.ToString().Split(" ")[0]), 2)}MB";
            videoSize.Inlines.Clear();
            videoSize.Inlines.Add(run);
        }
    }
}
