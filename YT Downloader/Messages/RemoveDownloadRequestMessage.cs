using YT_Downloader.ViewModels.Components;

namespace YT_Downloader.Messages
{
    public record RemoveDownloadRequestMessage(IDownloadableViewModel DownloadableViewModel);
}
