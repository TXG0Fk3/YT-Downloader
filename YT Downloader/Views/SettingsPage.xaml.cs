using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace YT_Downloader.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            LoadSettings();
        }

        // Carrega as configurações salvas na inicialização da página
        private void LoadSettings()
        {
            SetSelectedThemeRadioButton();
            showDefaultPath.Description = App.appSettings.DefaultDownloadsPath;
            askWhereSaveTS.IsOn = App.appSettings.AlwaysAskWhereSave;
        }

        // Define o botão de rádio selecionado com base no tema atual
        private void SetSelectedThemeRadioButton()
        {
            var selectedTheme = App.appSettings.Theme.ToString();
            var themeRadioButton = appThemeRadioBt.Items
                .Cast<RadioButton>()
                .FirstOrDefault(rb => rb.Tag?.ToString() == selectedTheme);

            if (themeRadioButton != null)
            {
                themeRadioButton.IsChecked = true;
            }
        }

        // Altera o tema do aplicativo
        private void Theme_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButtons radioButtons &&
                radioButtons.SelectedItem is RadioButton selectedRadioButton)
            {
                var selectedTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), selectedRadioButton.Tag.ToString());
                ApplyThemeAndSaveSettings(selectedTheme);
            }
        }

        // Aplica o tema e salva as configurações
        private void ApplyThemeAndSaveSettings(ElementTheme theme)
        {
            App.mainWindow.ApplyTheme(theme);
            App.appSettings.Theme = theme.ToString();
            App.appSettings.SaveNewSettings();
        }

        // Seleciona a pasta onde serão salvos os downloads
        private async void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new FolderPicker
            {
                FileTypeFilter = { "*" }
            };

            InitializeWithWindow.Initialize(openPicker, WindowNative.GetWindowHandle(App.mainWindow));
            var selectedFolder = await openPicker.PickSingleFolderAsync();

            if (selectedFolder != null)
            {
                UpdateDownloadPath(selectedFolder.Path);
            }
        }

        // Atualiza o caminho de download e salva as configurações
        private void UpdateDownloadPath(string path)
        {
            App.appSettings.DefaultDownloadsPath = path;
            showDefaultPath.Description = path;
            App.appSettings.SaveNewSettings();
        }

        // Define se o programa sempre deve perguntar ao usuário onde salvar os downloads
        private void AlwaysAskTS_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                App.appSettings.AlwaysAskWhereSave = toggleSwitch.IsOn;
                App.appSettings.SaveNewSettings();
            }
        }
    }
}
