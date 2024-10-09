using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;


namespace YT_Downloader.Controls
{
    public sealed partial class DownloadCard : UserControl
    {
        private readonly string DownloadPath;
        private readonly string FileName;
        private readonly string ThumbnailPath;
        private readonly Video Video;
        private readonly VideoOnlyStreamInfo VideoStreamInfo;
        private readonly AudioOnlyStreamInfo AudioStreamInfo;
        public CancellationToken token;

        public DownloadCard(string downloadPath,
                            string thumbnailPath,
                            Video video,
                            AudioOnlyStreamInfo audioStreamInfo,
                            VideoOnlyStreamInfo videoStreamInfo = null)
        {
            DownloadPath = downloadPath;
            ThumbnailPath = thumbnailPath;
            FileName = SanitizeFileName(video.Title);
            Video = video;
            VideoStreamInfo = videoStreamInfo;
            AudioStreamInfo = audioStreamInfo;
            token = new();

            InitializeComponent();

            Debug.WriteLine("L");

            LoadVideoInfo();

            DownloadAsync(token);
        }

        private string SanitizeFileName(string fileName) =>
            Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));

        private void LoadVideoInfo()
        {
            TitleHyperlink.Content = Video.Title;
            TitleHyperlink.NavigateUri = new Uri(Video.Url);
            VideoQualityAndSizeTextBlock.Text = $"{DownloadQuality} {Math.Round(DownloadSize, 2)} MB";

            ThumbnailBorder.Child = new Image
            {
                Source = new BitmapImage(new Uri(ThumbnailPath)),
                Stretch = Microsoft.UI.Xaml.Media.Stretch.Fill
            };
        }

        private async void DownloadAsync(CancellationToken token)
        {
            try
            {
                await (VideoStreamInfo != null
                    ? DownloadVideoAsync(token)
                    : DownloadAudioAsync(token));
            }
            catch (Exception ex)
            {
                DownloadProgressBar.ShowError = true;
            }
        }

        private async Task DownloadVideoAsync(CancellationToken token)
        {
            var streamInfos = new IStreamInfo[] { AudioStreamInfo, VideoStreamInfo };

            await App.youtubeClient.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder($"{DownloadPath}\\{FileName}.mp4").Build(),
                new Progress<double>(p => { DownloadProgressBar.Value = p * 100; }), token);
        }

        private async Task DownloadAudioAsync(CancellationToken token)
        {
            await App.youtubeClient.Videos.Streams.DownloadAsync(AudioStreamInfo, $"{DownloadPath}\\{FileName}.mp3",
                new Progress<double>(p => { DownloadProgressBar.Value = p * 100; }), token);
        }

        private string DownloadQuality
        {
            get
            {
                return VideoStreamInfo != null
                    ? VideoStreamInfo.VideoQuality.Label
                    : AudioStreamInfo.Bitrate.ToString();
            }
        }

        private double DownloadSize
        {
            get
            {
                return VideoStreamInfo != null
                    ? VideoStreamInfo.Size.MegaBytes + AudioStreamInfo.Size.MegaBytes
                    : AudioStreamInfo.Size.MegaBytes;
            }
        }
    }
}
