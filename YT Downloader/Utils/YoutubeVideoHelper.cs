using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YT_Downloader.Utils
{
    internal class YoutubeVideoHelper(string Url) : YoutubeClientHelper(Url)
    {
        public static async Task<YoutubeVideoHelper> CreateAsync(string Url)
        {
            App.Cts = new();
            var youtubeHelper = new YoutubeVideoHelper(Url);
            await youtubeHelper.LoadInfo(App.Cts.Token);
            return youtubeHelper;    
        }

        public override async Task LoadInfo(CancellationToken token)
        {
            _Video = await _YoutubeClient.Videos.GetAsync(_Url);
            if (token.IsCancellationRequested) return;

            _StreamManifest = await _YoutubeClient.Videos.Streams.GetManifestAsync(_Url);
        }

        public override string GetTitle() => _Video.Title;

        public HashSet<string> GetVideoResolutions()
        {
            return _StreamManifest.GetVideoOnlyStreams()
                .Where(s => s.Container == Container.Mp4)
                .Select(s => s.VideoQuality.Label)
                .ToHashSet();
        }

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

        public override async void DownloadAsync()
        {
            throw new System.NotImplementedException();
        }

        protected override async void DownloadPlaylistAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
