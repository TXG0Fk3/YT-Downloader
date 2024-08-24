using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;


namespace YT_Downloader.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            // Mostra as configurações já salvas
            appThemeRadioBt.SelectedIndex = App.appSettings.Theme;
            showDefaultPath.Description = App.appSettings.DefaultDownloadsPath;
            askWhereSaveTS.IsOn = App.appSettings.AlwaysAskWhereSave;
        }

        // Altera o tema do aplicativo
        private void Theme_SelectionChanged(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (sender as RadioButtons).SelectedItem as RadioButton;
            if (radioButton != null)
            {
                // Altera o tema atual
                byte theme = byte.Parse(radioButton.Tag.ToString());
                App.mainWindow.ApplyTheme(theme);
                App.appSettings.Theme = theme;

                // Salva as alterações no arquivo 
                App.appSettings.SaveNewSettings();
            }
        }

        // Seleciona a pasta onde serão salvos os downlods
        async private void SelectFolderButton_click(object sender, RoutedEventArgs e)
        {
            FolderPicker openPicker = new();
            openPicker.FileTypeFilter.Add("*");

            nint windowHandle = WindowNative.GetWindowHandle(App.mainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, windowHandle);

            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                App.appSettings.DefaultDownloadsPath = folder.Path;
                showDefaultPath.Description = folder.Path;

                // Salva as alterações
                App.appSettings.SaveNewSettings();
            }
        }

        // Define se o programa sempre deve perguntar ao usuário onde salvar os downloads,
        // Se estiver desativado, o programa utilizará o diretório padrão
        private void AlwaysAskTS_toggled(object sender, RoutedEventArgs e)
        {
            App.appSettings.AlwaysAskWhereSave = (sender as ToggleSwitch).IsOn;

            // Salva as alterações
            App.appSettings.SaveNewSettings();
        }
    }
}