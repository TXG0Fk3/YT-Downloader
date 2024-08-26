using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using static System.Net.Mime.MediaTypeNames;


namespace YT_Downloader.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            // Mostra as configurações já salvas
            appThemeRadioBt.Items.Cast<RadioButton>().FirstOrDefault(c => c?.Tag?.ToString() == App.appSettings.Theme.ToString()).IsChecked = true;
            showDefaultPath.Description = App.appSettings.DefaultDownloadsPath;
            askWhereSaveTS.IsOn = App.appSettings.AlwaysAskWhereSave;
        }

        // Altera o tema do aplicativo
        private void Theme_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // Altera o tema atual
            ElementTheme theme = (ElementTheme)Enum.Parse(typeof(ElementTheme), ((sender as RadioButtons).SelectedItem as RadioButton).Tag.ToString());
            App.mainWindow.ApplyTheme(theme);
            App.appSettings.Theme = theme.ToString();

            // Salva as alterações no arquivo 
            App.appSettings.SaveNewSettings();
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