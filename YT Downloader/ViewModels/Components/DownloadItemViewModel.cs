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
        public string Quality => _downloadItem.Quality;
        public string ThumbnailPath => _downloadItem.ThumbnailPath;

        public double Progress => _downloadItem.Progress;
        public DownloadStatus Status => _downloadItem.Status;
        public Exception Error => _downloadItem.Error;

        public string FormatedProgress => $"{Progress:00}%";

        public string FirstButtonIcon => Status != DownloadStatus.Error ? "\uE8DA" : "\uE72C";
        public string SecondButtonIcon => Status == DownloadStatus.Completed ? "\uE74D" : "\uF78A";

        public DownloadItemViewModel(DownloadItem downloadItem)
        {
            _downloadItem = downloadItem;

            downloadItem.StatusChanged += (_) => OnStatusChanged();
            downloadItem.ProgressChanged += (_) => OnPropertyChanged(nameof(Progress));
        }

        private void OnStatusChanged()
        {
            OnPropertyChanged(nameof(FirstButtonIcon));
            OnPropertyChanged(nameof(SecondButtonIcon));

            FirstButtonCommand.NotifyCanExecuteChanged();
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
    }
}
