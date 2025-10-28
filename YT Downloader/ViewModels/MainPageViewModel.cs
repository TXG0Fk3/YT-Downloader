using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using YT_Downloader.Models;
using YT_Downloader.Services;
using YT_Downloader.ViewModels.Components;

namespace YT_Downloader.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly DownloadsService _downloadsService;
        private readonly DialogService _dialogService;

        public ObservableCollection<IDownloadableViewModel> Downloads { get; private set; } = new();

        public bool IsDownloadItemsEmpty => Downloads.Count == 0;

        public MainPageViewModel(DownloadsService downloadsService, DialogService dialogService)
        {
            _downloadsService = downloadsService;
            _dialogService = dialogService;

            Downloads.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsDownloadItemsEmpty));
        }
    }
}
