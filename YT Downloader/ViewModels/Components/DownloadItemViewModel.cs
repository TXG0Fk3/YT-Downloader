using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using YT_Downloader.Enums;
using YT_Downloader.Models;

namespace YT_Downloader.ViewModels.Components
{
    public partial class DownloadItemViewModel : ObservableObject, IDownloadableViewModel
    {
        private readonly DownloadItem _downloadItem;

        public string Title => _downloadItem.Title;
        public string Author => _downloadItem.Author;
        public string Url => _downloadItem.Url;
        public string QualityAndSize => $"{_downloadItem.Quality} {_downloadItem.FileSizeMB:F1}MB";
        public string ThumbnailPath => _downloadItem.ThumbnailUrl;

        public DownloadStatus Status => _downloadItem.Status;
        public double Progress => _downloadItem.Progress;
        public string FormattedProgress => $"{Progress * 100:00}%";
        public string ProgressInfo
        {
            get
            {
                var t = _downloadItem.RemainingTime;
                var s = _downloadItem.FileSizeMB;

                return $"{_downloadItem.DownloadSpeedMBps:F1}MB/s | " +
                       $"~{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2} remaining | " +
                       $"{s * Progress:F1}MB / {s:F1}MB";
            }
        }
        public bool IsProgressInfoVisible => Status == DownloadStatus.Downloading;

        public Exception? Error => _downloadItem.Error;
        public bool IsErrorVisible => Status == DownloadStatus.Error;

        public string FirstButtonIcon => Status != DownloadStatus.Error ? "\uE8DA" : "\uE72C";
        public string SecondButtonIcon => Status == DownloadStatus.Completed ? "\uE74D" : "\uF78A";

        public DownloadItemViewModel(DownloadItem downloadItem)
        {
            _downloadItem = downloadItem;

            _downloadItem.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DownloadItem.Progress))
                {
                    OnPropertyChanged(nameof(Progress));
                    OnPropertyChanged(nameof(FormattedProgress));
                    OnPropertyChanged(nameof(ProgressInfo));
                }

                if (e.PropertyName == nameof(DownloadItem.Status))
                {
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(IsProgressInfoVisible));
                    OnPropertyChanged(nameof(IsErrorVisible));
                    OnPropertyChanged(nameof(FirstButtonIcon));
                    OnPropertyChanged(nameof(SecondButtonIcon));
                    FirstButtonCommand.NotifyCanExecuteChanged();
                }
            };
        }

        [RelayCommand(CanExecute = nameof(CanFirstButton))]
        private void OnFirstButton()
        {
            if (Status == DownloadStatus.Error)
                OnRetry();
            else if (Status == DownloadStatus.Completed)
                OnOpenLocal();
        }

        private bool CanFirstButton() =>
            Status is DownloadStatus.Completed or DownloadStatus.Error;

        [RelayCommand]
        private void OnSecondButton()
        {
            if (Status == DownloadStatus.Completed)
                OnDelete();
            else
                OnCancel();
        }

        private void OnCancel() { }
        private void OnDelete() { }
        private void OnOpenLocal() { }
        private void OnRetry() { }
        private void OnSeeLog() { }
    }
}
