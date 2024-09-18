using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YT_Downloader.Utils
{
    public abstract class YoutubeClientHelper
    {
        protected YoutubeClient _YoutubeClient = new();
        protected string _ThumbnailsDirectoryPath = $"{Path.GetTempPath()}\\TempThumbnails\\";
        protected string[] _Thumbnails;
        protected Video[] _Videos;
        protected StreamManifest[] _StreamManifests;
        protected YoutubeExplode.Playlists.Playlist _Playlist;
        public bool IsPlaylist;

        protected readonly string _Url;


        public YoutubeClientHelper(string Url)
        {
            _Url = Url;
            IsPlaylist = Url.Contains("playlist");
        }

        public abstract Task LoadInfo(CancellationToken token);

        public abstract double GetSize(string X);

        protected abstract double GetStreamSize(Index i, string X);

        protected abstract double GetPlaylistSize(string X);

        public async Task<string> GetThumbnailAsync()
        {
            if (!Directory.Exists(_ThumbnailsDirectoryPath))
                Directory.CreateDirectory(_ThumbnailsDirectoryPath);

            if (IsPlaylist)
                return await GetThumbnailAsync(0);
            else
            {
                foreach (var video in _Videos)
                {
                    
                }
            }
        }

        protected async Task<string> GetThumbnailAsync(Index i)
        {
            if (_ThumbnailsDirectoryPath == null)
            {
                ThumbnailPath = $"{Path.GetTempPath()}\\{_Videos[i].Id}.jpg";
                string thumbnailUrl = $"https://img.youtube.com/vi/{_Videos[i].Id}/mqdefault.jpg";

                using var httpClient = new HttpClient();
                var content = await httpClient.GetByteArrayAsync(thumbnailUrl);
                await File.WriteAllBytesAsync(_ThumbnailPath, content);
            }
            
            return _Thumbnails;
        }

        public void DeleteThumbnail()
        {
            if (_ThumbnailPath != null) File.Delete(_ThumbnailPath);
            _ThumbnailPath = null ;
        }

        public abstract string GetTitle();

        public abstract void DownloadAsync(string X);

        protected abstract void DownloadStreamAsync(string X);

        protected abstract void DownloadPlaylistAsync(string X);
    }
}
