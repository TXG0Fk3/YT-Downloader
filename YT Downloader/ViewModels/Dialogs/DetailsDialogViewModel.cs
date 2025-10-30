using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
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
        private VideoOnlyStreamInfo? _videoStreamInfo;
        private AudioOnlyStreamInfo? _audioStreamInfo;
        private CancellationTokenSource? _cts;

        [ObservableProperty] private string _urlBoxText = string.Empty;
        [ObservableProperty] private string _title = string.Empty;
        [ObservableProperty] private string _contentUrl = string.Empty;
        [ObservableProperty] private string _thumbnailPath = string.Empty;
        [ObservableProperty] private string _defaultFileName = string.Empty;
        [ObservableProperty] private string _userFileName = string.Empty;
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
            IsContentLoaded = false;
            IsErrorVisible = false;
            SelectedFormat = "Mp4";
            Title = "Loading...";
            DefaultFileName = "Loading...";
            UserFileName = string.Empty;
            SizeMB = "Loading...";
            ContentUrl = UrlBoxText;

            IsContentLoading = true;
            try
            {
                if (UrlBoxText.Contains("playlist?list="))
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
            _video = await _youtubeService.GetVideoAsync(UrlBoxText, _cts.Token);

            Title = _video.Title;
            DefaultFileName = SanitizeFileName(_video.Title);

            _streamManifest = await _youtubeService.GetStreamManifestAsync(_video.Id, _cts.Token);
            ThumbnailPath = await ThumbHelper.DownloadThumbnailAsync(_video.Id);

            UpdateAvailableQualities();   
        }

        private async Task LoadPlaylistInfoAsync()
        {
            IsPlaylist = true;
        }

        private void UpdateAvailableQualities()
        {
            if (SelectedFormat == "Mp4")
            {
                if (IsPlaylist)
                {
                    AvailableQualities = new HashSet<string>
                    {
                        "2160p60", "2160p", "1440p60", "1440p",
                        "1080p60", "1080p", "720p60", "720p",
                        "480p60", "480p", "360p60", "360p",
                        "240p60", "240p", "144p60", "144p"
                    };
                }
                else
                {
                    if (_streamManifest != null)
                    {
                        AvailableQualities = _streamManifest
                            .GetVideoOnlyStreams()
                            .Where(s => s.Container == Container.Mp4)
                            .Select(s => s.VideoQuality.Label)
                            .ToHashSet();
                    }   
                }
            }
            else AvailableQualities = ["Best"];

            SelectedQuality = AvailableQualities.FirstOrDefault();
        }

        private void UpdateSize()
        {
            if (IsPlaylist) SizeMB = "Undetermined";
            else if (_videoStreamInfo != null && _audioStreamInfo != null)
            {
                SizeMB = SelectedFormat == "Mp4" 
                    ? $"{(_videoStreamInfo.Size.MegaBytes + _audioStreamInfo.Size.MegaBytes):F1} MB"
                    : $"{_audioStreamInfo.Size.MegaBytes:F1} MB";
            }
        }

        async partial void OnSelectedFormatChanged(string? value)
        {
            if (value != null)
                UpdateAvailableQualities();
        }

        async partial void OnSelectedQualityChanged(string? value)
        {   
            if (value != null)
            {
                if (!IsPlaylist && _streamManifest != null)
                {
                    if (SelectedFormat == "Mp4")
                    _videoStreamInfo = _youtubeService.GetVideoOnlyStreamInfo(_streamManifest, value);

                    _audioStreamInfo = _youtubeService.GetBestAudioOnlyStreamInfo(_streamManifest);
                }

                UpdateSize();
            } 
        }

        private static string SanitizeFileName(string name) =>
            string.Concat(name.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
    } 
}
