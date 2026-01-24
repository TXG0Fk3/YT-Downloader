using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using YT_Downloader.Enums;

namespace YT_Downloader.Models
{
    public partial class DownloadGroup : ObservableObject, IDownloadable
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DownloadType Type { get; set; }
        public string Quality { get; set; }
        public string OutputPath { get; set; }
        public ObservableCollection<DownloadItem> Items { get; } = new();
        public CancellationTokenSource CTS { get; private set; } = new();
        [ObservableProperty] public partial Exception? Error { get; set; }

        public double Progress => Items.Count == 0 ? 0 : Items.Average(i => i.Progress);

        public DownloadStatus Status => Items.Where(i => i.Status is not DownloadStatus.Error and not DownloadStatus.Cancelled)
            .All(i => i.Status == DownloadStatus.Completed)
                ? DownloadStatus.Completed
                : Items.Any(i => i.Status is DownloadStatus.Downloading or DownloadStatus.Converting)
                    ? DownloadStatus.Downloading
                    : DownloadStatus.Pending;

        public DownloadGroup() =>
            Items.CollectionChanged += OnItemsChanged;

        private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (DownloadItem item in e.NewItems)
                    item.PropertyChanged += OnItemPropertyChanged;

            if (e.OldItems != null)
                foreach (DownloadItem item in e.OldItems)
                    item.PropertyChanged -= OnItemPropertyChanged;

            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(Status));
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DownloadItem.Progress))
                OnPropertyChanged(nameof(Progress));

            if (e.PropertyName == nameof(DownloadItem.Status))
                OnPropertyChanged(nameof(Status));
        }
    }
}
