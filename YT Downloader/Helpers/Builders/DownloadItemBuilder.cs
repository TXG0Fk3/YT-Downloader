using YT_Downloader.Enums;
using YT_Downloader.Models;
using YT_Downloader.Models.Info;

namespace YT_Downloader.Helpers.Builders
{
    public class DownloadItemBuilder
    {
        private readonly DownloadItem _item = new();

        public DownloadItemBuilder FromVideoInfo(VideoInfo video)
        {
            _item.VideoId = video.Id;
            _item.Url = video.Url;
            _item.Title = video.Title;
            _item.Author = video.Author;
            _item.ThumbnailUrl = video.ThumbnailUrl;
            _item.Manifest = video.Manifest;
            return this;
        }

        public DownloadItemBuilder WithOutputPath(string path)
        {
            _item.OutputPath = path;
            return this;
        }

        public DownloadItemBuilder AsVideo(string quality, StreamOption videoStream, StreamOption audioStream)
        {
            _item.Type = DownloadType.Video;
            _item.Quality = quality;
            _item.VideoStreamOption = videoStream;
            _item.AudioStreamOption = audioStream;
            return this;
        }

        public DownloadItemBuilder AsAudio(StreamOption audioStream)
        {
            _item.Type = DownloadType.Audio;
            _item.Quality = "Best";
            _item.AudioStreamOption = audioStream;
            return this;
        }

        public DownloadItem Build() => _item;
    }
}
