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

            // Mostra as configura��es j� salvas
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

                // Salva as altera��es no arquivo 
                App.appSettings.SaveNewSettings();
            }
        }

        // Seleciona a pasta onde ser�o salvos os downlods
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

                // Salva as altera��es
                App.appSettings.SaveNewSettings();
            }
        }

        // Define se o programa sempre deve perguntar ao usu�rio onde salvar os downloads,
        // Se estiver desativado, o programa utilizar� o diret�rio padr�o
        private void AlwaysAskTS_toggled(object sender, RoutedEventArgs e)
        {
            App.appSettings.AlwaysAskWhereSave = (sender as ToggleSwitch).IsOn;

            // Salva as altera��es
            App.appSettings.SaveNewSettings();
        }
    }
}