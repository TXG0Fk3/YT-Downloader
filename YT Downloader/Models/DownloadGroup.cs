using System.Collections.Generic;
using System.Linq;
using YT_Downloader.Enums;

namespace YT_Downloader.Models
{
    public class DownloadGroup : IDownloadable
    {
        public string Title { get; set; }
        public List<DownloadItem> Items { get; } = new();

        public double Progress => Items.Count == 0 ? 0 : Items.Average(i => i.Progress);

        public DownloadStatus Status =>
            Items.All(i => i.Status == DownloadStatus.Completed)
                ? DownloadStatus.Completed
                : Items.Any(i => i.Status == DownloadStatus.Downloading)
                    ? DownloadStatus.Downloading
                    : DownloadStatus.Pending;
    }
}
