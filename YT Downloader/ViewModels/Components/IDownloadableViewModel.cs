using System;
using YT_Downloader.Enums;

namespace YT_Downloader.ViewModels.Components
{
    public interface IDownloadableViewModel : IDisposable
    { 
        string Title { get; }
        double Progress { get; }
        DownloadStatus Status { get; }
    }
}
