using System;
using System.Collections.Generic;
using System.Threading;
using YT_Downloader.Enums;

namespace YT_Downloader.Models
{
    public interface IDownloadable
    {
        string Id { get; }
        DownloadType Type { get; }
        double Progress { get; }
        DownloadStatus Status { get; }
        Exception? Error { get; }
        CancellationTokenSource CTS { get; }
    }
}
