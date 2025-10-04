using System;
using System.Threading.Tasks;
using YoutubeExplode.Videos.Streams;
using YT_Downloader.Enums;

namespace YT_Downloader.Models
{
    public class DownloadItem
    {
        private DateTime? _startTime;

        public string VideoId { get; set; }
        public VideoOnlyStreamInfo VideoStreamInfo { get; set; }
        public AudioOnlyStreamInfo AudioStreamInfo { get; set; }
        public string OutputPath { get; set; }
        public double Progress { get; private set; } = 0.0;
        public TimeSpan? RemainingTime
        {
            get
            {
                if (!_startTime.HasValue || Progress <= 0)
                    return null;

                var elapsedTime = DateTime.Now - _startTime.Value;
                var totalSeconds = elapsedTime.TotalSeconds / Progress;
                var remainingSeconds = totalSeconds * (1 - Progress);
                return TimeSpan.FromSeconds(remainingSeconds);
            }
        }
        
        public DownloadStatus Status { get; set; } = DownloadStatus.Pending;
        public Exception Error { get; private set; }
        public DownloadType Type { get; set; }
        public Task DownloadTask { get; set; }

        public double FileSizeMB =>
            Type == DownloadType.Video
                ? (VideoStreamInfo?.Size.MegaBytes ?? 0) + (AudioStreamInfo?.Size.MegaBytes ?? 0)
                : (AudioStreamInfo?.Size.MegaBytes ?? 0);

        public double DownloadSpeedMBps
        {
            get
            {
                if (!_startTime.HasValue || Progress <= 0)
                    return 0.0;

                var elapsedTime = DateTime.Now - _startTime.Value;
                var downloadedMB = FileSizeMB * Progress;
                return downloadedMB / elapsedTime.TotalSeconds;
            }
        }

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

        public void MarkAsDownloading()
        {
            if (!_startTime.HasValue)
                _startTime = DateTime.Now;
            Status = DownloadStatus.Downloading;
        }

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