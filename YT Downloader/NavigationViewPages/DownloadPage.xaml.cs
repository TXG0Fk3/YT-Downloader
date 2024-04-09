using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Converter;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using System.Threading;
using Windows.Media.Protection.PlayReady;
using Microsoft.UI.Xaml.Media.Animation;


namespace YT_Downloader.NavigationViewPages
{
    public sealed partial class DownloadPage : Page
    {
        public static Frame view;
        public static YoutubeClient youtube;
        public static YoutubeExplode.Videos.Video video;
        public static string downloadType;
        public static VideoOnlyStreamInfo videoStreamInfo;
        public static IStreamInfo audioStreamInfo;

        public DownloadPage()
        {
            this.InitializeComponent();
            videoTitle.Text = video.Title.Length > 68 ? $"{video.Title[..68]}..." : video.Title;
            videoPicture.Source = new BitmapImage(new Uri($"{Path.GetTempPath()}\\{video.Id}.jpg"));

            DownloadVideo(App.cts.Token);
        }

        async void DownloadVideo(CancellationToken token)
        {
            var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };
            var totalSize = streamInfos.Sum(s => float.Parse(s.Size.Bytes.ToString().Split(" ")[0]));
            var totalSizeMb = totalSize / (1024 * 1024);
            var startTime = DateTime.Now;

            switch(downloadType)
            {
                case "V":
                    await youtube.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder($"D:\\YoutubeVideo\\{video.Title.Replace("\\", "").Replace("<", "").Replace(">", "").Replace(":", "").Replace("*", "").Replace("?", "").Replace("\"", "").Replace("/", "").Replace("|", "")}.mp4").Build(), new Progress<double>(p =>
                    {
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

                case "M":
                    break;

                case "P":
                    break;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.cts.Cancel();
            view.Navigate(typeof(NavigationViewPages.Video.VideoPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
    }
}
