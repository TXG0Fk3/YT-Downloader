using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using System;

namespace YT_Downloader.Utils
{
    class YoutubeVideoHelper(string Url) : YoutubeClientHelper(Url)
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
            _Videos.Append(await _YoutubeClient.Videos.GetAsync(_Url));
            if (token.IsCancellationRequested) return;

            _StreamManifests.Append(await _YoutubeClient.Videos.Streams.GetManifestAsync(_Url));
        }

        public override string GetTitle() => (IsPlaylist) ? _Playlist.Title : _Videos[0].Title;

        public HashSet<string> GetStreamResolutions()
        {
            if (IsPlaylist)
            {
                return
                [
                    "4320p", "2160p", "1440p", "1080p", "720p", "480p", "360p", "240p", "144p"
                ];
            }
            else
            {
                return _StreamManifests[0]
                    .GetVideoOnlyStreams()
                    .Where(s => s.Container == Container.Mp4)
                    .Select(s => s.VideoQuality.Label)
                    .ToHashSet();
            }
        }

        public override double GetSize(string resolution) =>
            (IsPlaylist) ? GetPlaylistSize(resolution) : GetStreamSize(resolution);

        protected override double GetStreamSize(Index i, string resolution)
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

        protected override double GetPlaylistSize(string resolution)
        {
            double playlistSize = 0;

            foreach (var video in _Videos)

            return selectedStream.Size.MegaBytes + audioStream.Size.MegaBytes;
        }

        public override async void DownloadAsync(string resolution)
        {
            throw new System.NotImplementedException();
        }

        protected override async void DownloadStreamAsync(string resolution)
        {
            throw new System.NotImplementedException();
        }

        protected override async void DownloadPlaylistAsync(string resolution)
        {
            throw new System.NotImplementedException();
        }
    }
}
