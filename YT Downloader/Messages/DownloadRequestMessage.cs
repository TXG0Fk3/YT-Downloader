using YT_Downloader.Models;

namespace YT_Downloader.Messages
{
    public record DownloadRequestMessage(IDownloadable DownloadInfo);
}
