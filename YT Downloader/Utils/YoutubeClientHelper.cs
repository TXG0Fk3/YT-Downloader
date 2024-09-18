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
        protected StreamManifest _StreamManifest;
        protected string _ThumbnailPath;
        protected Video _Video;
        public bool IsPlaylist;

        protected readonly string _Url;


        public YoutubeClientHelper(string Url)
        {
            _Url = Url;
            IsPlaylist = Url.Contains("playlist");
        }

        public abstract Task LoadInfo(CancellationToken token);

        public async Task<string> GetThumbnailAsync()
        {
            if (_ThumbnailPath == null)
            {
                _ThumbnailPath = $"{Path.GetTempPath()}\\{_Video.Id}.jpg";
                string thumbnailUrl = $"https://img.youtube.com/vi/{_Video.Id}/mqdefault.jpg";

                using (var httpClient = new HttpClient())
                {
                    var content = await httpClient.GetByteArrayAsync(thumbnailUrl);
                    await File.WriteAllBytesAsync(_ThumbnailPath, content);
                }
            }
            
            return _ThumbnailPath;
        }

        public void DeleteThumbnail()
        {
            if (_ThumbnailPath != null) File.Delete(_ThumbnailPath);
            _ThumbnailPath = null ;
        }

        public abstract string GetTitle();

        public abstract void DownloadAsync();

        protected abstract void DownloadPlaylistAsync();
    }
}
