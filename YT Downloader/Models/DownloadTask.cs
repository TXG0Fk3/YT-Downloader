using System;

namespace YT_Downloader.Models
{
    public record DownloadTask(DownloadItem Item, IProgress<double> Progress);
}
