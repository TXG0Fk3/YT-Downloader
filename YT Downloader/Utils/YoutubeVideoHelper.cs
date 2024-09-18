using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using System;
using AngleSharp.Dom;

namespace YT_Downloader.Utils
{
    class YoutubeVideoHelper()
    {
        protected YoutubeClient _YoutubeClient = new();
        protected string _ThumbnailPath;
        protected Video _Video;
        protected StreamManifest[] _StreamManifests;
        protected YoutubeExplode.Playlists.Playlist _Playlist;
        public bool IsPlaylist;

        public YoutubeVideoHelper(string Url)
        {
            _Url = Url;
            IsPlaylist = Url.Contains("playlist");
        }

        public static async Task<YoutubeVideoHelper> CreateAsync(string Url)
        {
            App.Cts = new();
            var youtubeHelper = new YoutubeVideoHelper(Url);
            await youtubeHelper.LoadInfo(App.Cts.Token);
            return youtubeHelper;    
        }

        public async Task LoadInfo(CancellationToken token)
        {
            _Videos.Append(await _YoutubeClient.Videos.GetAsync(_Url));
            if (token.IsCancellationRequested) return;

            _StreamManifests.Append(await _YoutubeClient.Videos.Streams.GetManifestAsync(_Url));
        }

        public string GetTitle() => (IsPlaylist) ? _Playlist.Title : _Videos[0].Title;

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

        public double GetSize(string resolution) =>
            (IsPlaylist) ? GetPlaylistSize(resolution) : GetStreamSize(resolution);

        protected double GetStreamSize(Index i, string resolution)
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

        protected double GetPlaylistSize(string resolution)
        {
            double playlistSize = 0;

            foreach (var video in _Videos)

            return selectedStream.Size.MegaBytes + audioStream.Size.MegaBytes;
        }

        public async void DownloadAsync(string resolution)
        {
            throw new System.NotImplementedException();
        }

        protected async void DownloadStreamAsync(string resolution)
        {
            throw new System.NotImplementedException();
        }

        protected async void DownloadPlaylistAsync(string resolution)
        {
            throw new System.NotImplementedException();
        }
    }
}
