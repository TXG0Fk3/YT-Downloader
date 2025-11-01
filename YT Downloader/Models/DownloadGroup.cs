using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using YT_Downloader.Enums;

namespace YT_Downloader.Models
{
    public class DownloadGroup : ObservableObject, IDownloadable
    {
        public required string PlaylistId { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public required string Url { get; set; }
        public required DownloadType Type { get; set; }
        public required string Quality { get; set; }
        public required string OutputPath { get; set; }
        public ObservableCollection<DownloadItem> Items { get; } = new();

        public double Progress => Items.Count == 0 ? 0 : Items.Average(i => i.Progress);

        public DownloadStatus Status =>
            Items.All(i => i.Status == DownloadStatus.Completed)
                ? DownloadStatus.Completed
                : Items.Any(i => i.Status == DownloadStatus.Downloading)
                    ? DownloadStatus.Downloading
                    : DownloadStatus.Pending;

        private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (DownloadItem item in e.NewItems)
                {
                    item.PropertyChanged += OnItemPropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (DownloadItem item in e.OldItems)
                {
                    item.PropertyChanged -= OnItemPropertyChanged;
                }
            }

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
