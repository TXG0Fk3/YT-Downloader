using System.Collections.Generic;
using System.Linq;
using YT_Downloader.Enums;

namespace YT_Downloader.Models
{
    public class DownloadGroup : IDownloadable
    {
        public required string PlaylistId { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public required string Quality { get; set; }
        public List<DownloadItem> Items { get; set; } = new();

        public double Progress => Items.Count == 0 ? 0 : Items.Average(i => i.Progress);

        public DownloadStatus Status =>
            Items.All(i => i.Status == DownloadStatus.Completed)
                ? DownloadStatus.Completed
                : Items.Any(i => i.Status == DownloadStatus.Downloading)
                    ? DownloadStatus.Downloading
                    : DownloadStatus.Pending;
    }
}
