using CommunityToolkit.Mvvm.ComponentModel;
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
        }
    }
}
