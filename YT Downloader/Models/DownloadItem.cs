using System;
using YoutubeExplode.Videos.Streams;
using YT_Downloader.Enums;

namespace YT_Downloader.Models
{
    public class DownloadItem : IDownloadable
    {
        private DateTime? _startTime;

        public string VideoId { get; set; }
        public string Title { get; set; }
        public DownloadType Type { get; set; }
        public VideoOnlyStreamInfo VideoStreamInfo { get; set; }
        public AudioOnlyStreamInfo AudioStreamInfo { get; set; }
        public string OutputPath { get; set; }
        public double Progress { get; private set; } = 0.0;
        public DownloadStatus Status { get; private set; } = DownloadStatus.Pending;
        public Exception Error { get; private set; }

        public double FileSizeMB =>
            Type == DownloadType.Video
                ? (VideoStreamInfo?.Size.MegaBytes ?? 0) + (AudioStreamInfo?.Size.MegaBytes ?? 0)
                : (AudioStreamInfo?.Size.MegaBytes ?? 0);

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

        public event Action<DownloadItem> StatusChanged;
        public event Action<DownloadItem> ProgressChanged;

        public void UpdateProgress(double value)
        {
            Progress = Math.Clamp(value, 0.0, 1.0);
            ProgressChanged?.Invoke(this);
        }

        public void MarkAsDownloading()
        {
            if (!_startTime.HasValue)
                _startTime = DateTime.Now;
            SetStatus(DownloadStatus.Downloading);
        }

        public void MarkAsCompleted()
        {
            Progress = 1.0;
            SetStatus(DownloadStatus.Completed);
        }

        public void MarkAsCancelled() =>
            SetStatus(DownloadStatus.Cancelled);

        public void MarkAsError(Exception ex)
        {
            Error = ex;
            SetStatus(DownloadStatus.Error);
        }

        private void SetStatus(DownloadStatus status)
        {
            Status = status;
            StatusChanged?.Invoke(this);
        }
    }
}