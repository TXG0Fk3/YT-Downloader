using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using YT_Downloader.Messages;
using YT_Downloader.Models;
using YT_Downloader.Services;
using YT_Downloader.ViewModels.Components;

namespace YT_Downloader.ViewModels
{
    public partial class MainPageViewModel : ObservableObject,
        IRecipient<FolderPickerRequestMessage>, IRecipient<DownloadRequestMessage>,
        IRecipient<RetryDownloadRequestMessage>, IRecipient<RemoveDownloadRequestMessage>
    {
        private readonly DownloadsService _downloadsService;
        private readonly DialogService _dialogService;
        private readonly IMessenger _messenger;

        public ObservableCollection<IDownloadableViewModel> Downloads { get; private set; } = new();

        public bool IsDownloadItemsEmpty => Downloads.Count == 0;

        public MainPageViewModel(DownloadsService downloadsService, DialogService dialogService, IMessenger messenger)
        {
            _downloadsService = downloadsService;
            _dialogService = dialogService;
            _messenger = messenger;

            _messenger.RegisterAll(this);

            Downloads.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsDownloadItemsEmpty));
        }

        public async void Receive(FolderPickerRequestMessage message) =>
            message.Tcs.SetResult(await OnOpenFolderPickerAsync());

        public void Receive(DownloadRequestMessage message) =>
            OnEnqueueDownload(message.DownloadInfo);

        public void Receive(RetryDownloadRequestMessage message) =>
            OnRetryDownload(message.Item);

        public void Receive(RemoveDownloadRequestMessage message) =>
            OnRemoveDownload(message.DownloadableViewModel);

        [RelayCommand]
        private async Task OnAddDownloadAsync() =>
            await _dialogService.ShowDetailsDialogAsync();

        [RelayCommand]
        private async Task OnHelp() => throw new NotImplementedException();

        [RelayCommand]
        private async Task OnSettings() => throw new NotImplementedException();

        private void OnEnqueueDownload(IDownloadable downloadable)
        {
            if (downloadable is DownloadItem item)
                Downloads.Add(new DownloadItemViewModel(item));
            else if (downloadable is DownloadGroup group)
                Downloads.Add(new DownloadGroupViewModel(group));

            _ = _downloadsService.EnqueueDownloadable(downloadable);
        }

        private void OnRetryDownload(DownloadItem item) =>
            _ = _downloadsService.EnqueueDownloadable(item);

        private void OnRemoveDownload(IDownloadableViewModel vm) =>
            Downloads.Remove(vm);

        public async Task<string?> OnOpenFolderPickerAsync() =>
            await _dialogService.OpenFolderPickerAsync();
    }
}
