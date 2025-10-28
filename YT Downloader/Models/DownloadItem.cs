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
        public string Author { get; set; }
        public string Url { get; set; }
        public DownloadType Type { get; set; }
        public string Quality { get; set; }
        public VideoOnlyStreamInfo VideoStreamInfo { get; set; }
        public AudioOnlyStreamInfo AudioStreamInfo { get; set; }
        public string ThumbnailPath { get; set; }
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

        public void UpdateProgress(double value)
        {
            if (Math.Abs(value % 0.01) < 0.0001 || value == 1.0)
                Progress = Math.Clamp(value, 0.0, 1.0);
        }

        public void MarkAsDownloading()
        {
            if (!_startTime.HasValue)
                _startTime = DateTime.Now;
            Status = DownloadStatus.Downloading;
        }

        public void MarkAsCompleted()
        {
            Progress = 1.0;
            Status = DownloadStatus.Completed;
        }

        public void MarkAsCancelled() =>
            Status = DownloadStatus.Cancelled;

        public void MarkAsError(Exception ex)
        {
            Error = ex;
            Status = DownloadStatus.Error;
        }
    }
}