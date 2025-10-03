using System;
using System.Threading.Tasks;
using YT_Downloader.Enums;

namespace YT_Downloader.Models
{
    public class DownloadItem
    {
        public string VideoId { get; set; }
        public string OutputPath { get; set; }
        public double Progress { get; private set; }
        public DownloadStatus Status { get; set; } = DownloadStatus.Pending;
        public Exception Error { get; set; }
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
            Progress = value;
            ProgressChanged?.Invoke(this);
        }

        public void MarkAsDownloading() => Status = DownloadStatus.Downloading;
        public void MarkAsCompleted() => Status = DownloadStatus.Completed;
        public void MarkAsCancelled() => Status = DownloadStatus.Cancelled;
        public void MarkAsError(Exception ex)
        {
            Status = DownloadStatus.Error;
            Error = ex;
        }
    }
}