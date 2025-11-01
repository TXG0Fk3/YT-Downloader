using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading;
using YoutubeExplode.Videos.Streams;
using YT_Downloader.Enums;

namespace YT_Downloader.Models
{
    public partial class DownloadItem : ObservableObject, IDownloadable
    {
        private DateTime? _startTime;

        public required string VideoId { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public required string Url { get; set; }
        public required DownloadType Type { get; set; }
        public required string Quality { get; set; }
        public required VideoOnlyStreamInfo VideoStreamInfo { get; set; }
        public required AudioOnlyStreamInfo AudioStreamInfo { get; set; }
        public required string ThumbnailPath { get; set; }
        public required string OutputPath { get; set; }

        [ObservableProperty] private double progress = 0.0;
        [ObservableProperty] public DownloadStatus status = DownloadStatus.Pending;
        [ObservableProperty] public Exception? error;

        public IProgress<double> ProgressReporter { get; }
        public CancellationTokenSource CTS { get; private set; } = new();

        public DownloadItem() =>
            ProgressReporter = new Progress<double>(p => UpdateProgress(p));

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
            CTS = new();
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