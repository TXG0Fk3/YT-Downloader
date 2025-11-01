using YT_Downloader.Enums;

namespace YT_Downloader.Models
{
    public interface IDownloadable
    {
        string Title { get; }
        double Progress { get; }
        DownloadStatus Status { get; }
    }
}
