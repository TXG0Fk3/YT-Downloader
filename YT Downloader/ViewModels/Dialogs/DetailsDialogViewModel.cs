using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using YT_Downloader.Models;
using YT_Downloader.Services;
using YT_Downloader.Utils;

namespace YT_Downloader.ViewModels.Dialogs
{
    public partial class DetailsDialogViewModel : ObservableObject
    {
        private readonly YoutubeService _youtubeService;

        private IVideo? _video;
        private IPlaylist? _playlist;
        private StreamManifest? _streamManifest;
        private CancellationTokenSource? _cts;

        [ObservableProperty] private string _ContentUrl = string.Empty;
        [ObservableProperty] private string _title = string.Empty;
        [ObservableProperty] private string _thumbnailPath = string.Empty;
        [ObservableProperty] private string _sizeMB = string.Empty;

        [ObservableProperty] private IReadOnlyList<string> _availableFormats = ["Mp4", "Mp3"];
        [ObservableProperty] private HashSet<string> _availableQualities = new();

        [ObservableProperty] private string? _selectedFormat = "Mp4";
        [ObservableProperty] private string? _selectedQuality;

        [ObservableProperty] private bool _isPlaylist;

        [ObservableProperty, NotifyPropertyChangedFor(nameof(IsQualitySelectionEnabled), nameof(IsContentVisible), nameof(IsDownloadEnabled))]
        private bool _isContentLoading;
        [ObservableProperty] private bool _isContentLoaded;

        [ObservableProperty] private string _errorMessage = string.Empty;
        [ObservableProperty] private bool _isErrorVisible;

        private string _fileName = string.Empty;

        public bool IsQualitySelectionEnabled => SelectedFormat == "Mp4" && !IsContentLoading;
        public bool IsContentVisible => IsContentLoading || IsContentLoaded;
        public bool IsDownloadEnabled => IsContentLoaded && !IsContentLoading;

        public DetailsDialogViewModel(YoutubeService youtubeService)
        {
            _youtubeService = youtubeService;
        }

        public IDownloadable GetDownloadInfo() =>
            throw new NotImplementedException();

        [RelayCommand]
        private async Task LoadContentInfo()
        {
            if (string.IsNullOrWhiteSpace(ContentUrl))
                return;

            IsErrorVisible = false;
            IsContentLoading = true;
            try
            {
                if (ContentUrl.Contains("playlist?list="))
                    await LoadPlaylistInfoAsync();
                else
                    await LoadVideoInfoAsync();

                IsContentLoaded = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                IsErrorVisible = true;
            }
            finally
            {
                IsContentLoading = false;
            }
        }

        private async Task LoadVideoInfoAsync()
        {
            IsPlaylist = false;

            _cts = new CancellationTokenSource();
            _video = await _youtubeService.GetVideoAsync(ContentUrl, _cts.Token);
            _streamManifest = await _youtubeService.GetStreamManifestAsync(_video.Id, _cts.Token);

            Title = _video.Title;
            ThumbnailPath = await ThumbHelper.DownloadThumbnailAsync(_video.Id);

            AvailableQualities = GetAvailableQualities();
        }

        private async Task LoadPlaylistInfoAsync()
        {
            IsPlaylist = true;
        }

        private HashSet<string> GetAvailableQualities()
        {
            if (_streamManifest == null) return new();

            if (SelectedFormat == "Mp4")
            {
                return IsPlaylist
                    ? ["2160p60", "2160p", "1440p60", "1440p", "1080p60", "1080p", "720p60", "720p", "480p60", "480p", "360p60", "360p", "240p60", "240p", "144p60", "144p"]
                    : _streamManifest
                        .GetVideoOnlyStreams()
                        .Where(s => s.Container == Container.Mp4)
                        .Select(s => s.VideoQuality.Label)
                        .ToHashSet();
            }
            else return ["Best"];
        }
    } 
}
