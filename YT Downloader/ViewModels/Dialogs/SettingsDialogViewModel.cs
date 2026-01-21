using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Generic;
using System.Threading.Tasks;
using YT_Downloader.Enums;
using YT_Downloader.Messages;
using YT_Downloader.Models;
using YT_Downloader.Services;

namespace YT_Downloader.ViewModels.Dialogs
{
    public partial class SettingsDialogViewModel : ObservableObject
    {
        private readonly SettingsService _settingsService;
        private readonly DialogService _dialogService;
        private readonly IMessenger _messenger;

        public IReadOnlyList<ThemeOption> ThemeOptions { get; } =
            new List<ThemeOption>() {ThemeOption.Light, ThemeOption.Dark, ThemeOption.System};

        [ObservableProperty] private ThemeOption _selectedThemeOption;
        [ObservableProperty] private string _defaultDownloadsPath;
        [ObservableProperty] private bool _isAlwaysAskWhereSaveOn;

        public SettingsDialogViewModel(SettingsService settingsService, DialogService dialogService, IMessenger messenger)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _messenger = messenger;

            SelectedThemeOption = _settingsService.Current.Theme;
            DefaultDownloadsPath = _settingsService.Current.DefaultDownloadsPath;
            IsAlwaysAskWhereSaveOn = _settingsService.Current.AlwaysAskWhereSave;
        }

        [RelayCommand]
        private void OnSelectTheme() =>
            SaveSettings();

        [RelayCommand]
        private async Task OnSelectDefaultDownloadsFolder()
        {
            var path = await _dialogService.OpenFolderPickerAsync();
            if (!string.IsNullOrEmpty(path))
            {
                DefaultDownloadsPath = path;
                SaveSettings();
            }
        }

        [RelayCommand]
        private void OnAlwaysAskWhereSave() =>
            SaveSettings();

        private void SaveSettings()
        {
            var newSettings = new AppSettings(
                    Theme: SelectedThemeOption,
                    DefaultDownloadsPath: DefaultDownloadsPath,
                    AlwaysAskWhereSave: IsAlwaysAskWhereSaveOn
                );

            _settingsService.Save(newSettings);
            _messenger.Send(new ChangeThemeRequestMessage(SelectedThemeOption));
        }
    }
}
