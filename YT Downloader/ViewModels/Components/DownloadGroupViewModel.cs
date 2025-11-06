using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using YT_Downloader.Enums;
using YT_Downloader.Helpers;
using YT_Downloader.Messages;
using YT_Downloader.Models;

namespace YT_Downloader.ViewModels.Components
{
    public partial class DownloadGroupViewModel : ObservableObject, IDownloadableViewModel,
        IRecipient<RemoveDownloadRequestMessage>
    {
        private readonly DownloadGroup _downloadGroup;
        private readonly IMessenger _messenger;

        public string Title => _downloadGroup.Title;
        public string Author => _downloadGroup.Author;
        public string Url => _downloadGroup.Url;
        public string Quality => _downloadGroup.Quality;

        public DownloadStatus Status => _downloadGroup.Status;
        public double Progress => _downloadGroup.Progress;
        public string FormattedProgress => $"{Progress * 100:00}%";

        public ObservableCollection<DownloadItemViewModel> Items { get; } = new();
        public bool IsLoadingCardVisible => Items.Count == 0 && !IsErrorVisible;

        public Exception? Error => _downloadGroup.Error;
        public bool IsErrorVisible => Error != null;

        public string SecondButtonIcon => Status == DownloadStatus.Completed ? "\uE74D" : "\uF78A";

        public DownloadGroupViewModel(DownloadGroup downloadGroup, IMessenger messenger)
        {
            _downloadGroup = downloadGroup;
            _messenger = messenger;

            _messenger.RegisterAll(this);

            _downloadGroup.PropertyChanged += OnGroupPropertyChanged;
            _downloadGroup.Items.CollectionChanged += OnItemsChanged;
        }

        public void Receive(RemoveDownloadRequestMessage message)
        {
            if (message.DownloadableViewModel is DownloadItemViewModel itemVM)
                OnRemoveItemRequested(itemVM, message.Downloadable as DownloadItem);
        }

        [RelayCommand]
        private void OnOpenLocal() =>
            FileHelper.OpenFolder(_downloadGroup.OutputPath);

        [RelayCommand]
        private void OnDelete()
        {
            _downloadGroup.CTS.Cancel();
            _downloadGroup.PropertyChanged -= OnGroupPropertyChanged;
            _downloadGroup.Items.CollectionChanged -= OnItemsChanged;

            _messenger.Send(new RemoveDownloadRequestMessage(this));
        }

        [RelayCommand]
        private void OnSeeLog() =>
            _messenger.Send(new ErrorDialogRequestMessage(Error?.Message ?? "No Log"));

        private void OnRemoveItemRequested(DownloadItemViewModel itemViewModel, DownloadItem? item)
        {
            Items.Remove(itemViewModel);

            if (item != null)
                _downloadGroup.Items.Remove(item);

            if (Items.Count == 0) OnDelete();
        }

        private void OnGroupPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DownloadGroup.Progress))
            {
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(FormattedProgress));
            }

            if (e.PropertyName == nameof(DownloadGroup.Status))
            {
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(IsLoadingCardVisible));
                OnPropertyChanged(nameof(SecondButtonIcon));
            }

            if (e.PropertyName == nameof(DownloadGroup.Error))
            {
                OnPropertyChanged(nameof(IsLoadingCardVisible));
                OnPropertyChanged(nameof(IsErrorVisible));
            }
        }

        private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (DownloadItem item in e.NewItems)
                    Items.Add(new DownloadItemViewModel(item, _messenger));

            OnPropertyChanged(nameof(IsLoadingCardVisible));
        }
    }
}
