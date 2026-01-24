using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.ComponentModel;
using YT_Downloader.Enums;
using YT_Downloader.Helpers;
using YT_Downloader.Messages;
using YT_Downloader.Models;

namespace YT_Downloader.ViewModels.Components
{
    public partial class DownloadItemViewModel : ObservableObject, IDownloadableViewModel
    {
        private readonly DownloadItem _downloadItem;
        private readonly IMessenger _messenger;

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
        public bool IsDownloading => Status == DownloadStatus.Downloading;
        public bool IsConverting => Status == DownloadStatus.Converting;

        public Exception? Error => _downloadItem.Error;
        public bool IsErrorVisible => Status == DownloadStatus.Error;

        public string FirstButtonIcon => Status != DownloadStatus.Error ? "\uE8DA" : "\uE72C";
        public string SecondButtonIcon => Status == DownloadStatus.Completed ? "\uE74D" : "\uF78A";

        public DownloadItemViewModel(DownloadItem downloadItem, IMessenger messenger)
        {
            _downloadItem = downloadItem;
            _messenger = messenger;

            _downloadItem.PropertyChanged += OnDownloadItemPropertyChanged;
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

        [RelayCommand]
        private void OnSeeLog() =>
            _messenger.Send(new ErrorDialogRequestMessage(Error?.Message ?? "No Log"));

        private void OnCancel()
        {
            _downloadItem.MarkAsCancelled();
            Cleanup();
            _messenger.Send(new RemoveDownloadRequestMessage(this, _downloadItem));
            _messenger.UnregisterAll(this);
        }

        private void OnDelete()
        {
            FileHelper.DeleteFile(_downloadItem.OutputPath);
            Cleanup();
            _messenger.Send(new RemoveDownloadRequestMessage(this, _downloadItem));
            _messenger.UnregisterAll(this);
        }

        private void OnOpenLocal() =>
            FileHelper.OpenFolder(_downloadItem.OutputPath);

        private void OnRetry() =>
            _messenger.Send(new RetryDownloadRequestMessage(_downloadItem));

        private void Cleanup() =>
            _downloadItem.PropertyChanged -= OnDownloadItemPropertyChanged;

        private void OnDownloadItemPropertyChanged(object? s, PropertyChangedEventArgs e)
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
                OnPropertyChanged(nameof(IsDownloading));
                OnPropertyChanged(nameof(IsConverting));
                OnPropertyChanged(nameof(IsErrorVisible));
                OnPropertyChanged(nameof(FirstButtonIcon));
                OnPropertyChanged(nameof(SecondButtonIcon));
                FirstButtonCommand.NotifyCanExecuteChanged();
            }
        }
    }
}
