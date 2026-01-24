using System;
using System.Threading;
using YT_Downloader.Enums;

namespace YT_Downloader.Models
{
    public interface IDownloadable
    {
        string Id { get; }
        string Title { get; }
        string Author { get; }
        DownloadType Type { get; }
        string Quality { get; }
        string OutputPath { get; }
        double Progress { get; }
        DownloadStatus Status { get; }
        Exception? Error { get; }
        CancellationTokenSource CTS { get; }
    }
}
