using YT_Downloader.Models;
using YT_Downloader.ViewModels.Components;

namespace YT_Downloader.Messages
{
    public record RemoveDownloadRequestMessage(IDownloadableViewModel DownloadableViewModel, IDownloadable? Downloadable = null);
}
