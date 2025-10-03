using System;
using System.Threading.Tasks;
using YT_Downloader.Enums;

namespace YT_Downloader.Models
{
    public class DownloadItem
    {
        public string VideoId { get; set; }
        public string OutputPath { get; set; }
        public double Progress { get; private set; } = 0.0;
        public DownloadStatus Status { get; set; } = DownloadStatus.Pending;
        public Exception Error { get; private set; }
        public DownloadType Type { get; set; }
        public Task DownloadTask { get; set; }

        public event Action<DownloadItem> Completed;
        public event Action<DownloadItem> ProgressChanged;

        internal void OnCompleted()
        {
            Status = DownloadStatus.Completed;
            Completed?.Invoke(this);
        }

        public void UpdateProgress(double value)
        {
            Progress = Math.Clamp(value, 0.0, 1.0);
            ProgressChanged?.Invoke(this);
        }

        public void MarkAsDownloading() => Status = DownloadStatus.Downloading;

        public void MarkAsCompleted() => OnCompleted();

        public void MarkAsCancelled()
        {
            Status = DownloadStatus.Cancelled;
            Completed?.Invoke(this);
        }

        public void MarkAsError(Exception ex)
        {
            Status = DownloadStatus.Error;
            Error = ex;
            Completed?.Invoke(this);
        }
    }
}