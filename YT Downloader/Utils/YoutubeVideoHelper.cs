using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using Microsoft.UI.Xaml.Controls;
using System;
using YoutubeExplode.Converter;
using YT_Downloader.Views;

namespace YT_Downloader.Utils
{
    class YoutubeVideoHelper
    {
        private YoutubeClient _YoutubeClient = new();
        private string _ThumbnailPath;
        private Video _Video;
        private StreamManifest _StreamManifest;

        private readonly string _Url;

        public YoutubeVideoHelper(string url) =>
            _Url = url;

        public static async Task<YoutubeVideoHelper> CreateAsync(string Url)
        {
            App.Cts = new();
            var youtubeHelper = new YoutubeVideoHelper(Url);
            await youtubeHelper.LoadInfo(App.Cts.Token);
            return youtubeHelper;    
        }

        public async Task LoadInfo(CancellationToken token)
        {
            _Video = await _YoutubeClient.Videos.GetAsync(_Url);
            if (token.IsCancellationRequested) return;

            _StreamManifest = await _YoutubeClient.Videos.Streams.GetManifestAsync(_Url);
        }

        public string GetTitle() => _Video.Title;

        public HashSet<string> GetResolutions() =>
            _StreamManifest
                .GetVideoOnlyStreams()
                .Where(s => s.Container == Container.Mp4)
                .Select(s => s.VideoQuality.Label)
                .ToHashSet();

        public double GetSize(string resolution)
        {
            var selectedStream = _StreamManifest
                .GetVideoOnlyStreams()
                .Where(s => s.Container == Container.Mp4)
                .First(s => s.VideoQuality.Label == resolution);

            var audioStream = _StreamManifest.GetAudioOnlyStreams()
                .Where(s => s.Container == Container.Mp4)
                .GetWithHighestBitrate();

            return selectedStream.Size.MegaBytes + audioStream.Size.MegaBytes;
        }

        public async void DownloadAsync(string resolution, DateTime startTime, ProgressBar progressBar = null)
        {
            var videoStream = _StreamManifest
                .GetVideoOnlyStreams()
                .Where(s => s.Container == Container.Mp4)
                .First(s => s.VideoQuality.Label == resolution);

            var audioStream = _StreamManifest.GetAudioOnlyStreams()
                .Where(s => s.Container == Container.Mp4)
                .GetWithHighestBitrate();

            var streamInfos = new IStreamInfo[] { audioStream, videoStream };
            var totalSizeMb = streamInfos.Sum(s => s.Size.Bytes / (1024 * 1024f));

            await _YoutubeClient.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder($"{DownloadPath}\\{FileName}.mp4").Build(),
                new Progress<double>(p => { if (p % 0.005 < 0.0001) { UpdateProgress(p, totalSizeMb, startTime); } }), token);

        }
    }
}
