using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Text.Json;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;


namespace YT_Downloader.NavigationViewPages
{
    public sealed partial class SettingsPage : Page
    {
        public static Grid rootElement;

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
                string theme = radioButton.Content.ToString();
                switch (theme)
                {
                    case "Light": // Claro
                        rootElement.RequestedTheme = ElementTheme.Light;
                        App.appSettings.Theme = 0;
                        break;

                    case "Dark": // Escuro
                        rootElement.RequestedTheme = ElementTheme.Dark;
                        App.appSettings.Theme = 1;
                        break;

                    case "System": // Padrão do Sistema
                        rootElement.RequestedTheme = ElementTheme.Default;
                        App.appSettings.Theme = 2;
                        break;
                }

                // Salva as alterações
                SaveNewSettings();
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
                SaveNewSettings();
            }
        }

        // Define se o programa sempre deve perguntar ao usuário onde salvar os downloads,
        // Se estiver desativado, o programa utilizará o diretório padrão
        private void AlwaysAskTB_toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;

            App.appSettings.AlwaysAskWhereSave = toggleSwitch.IsOn;

            // Salva as alterações
            SaveNewSettings();
        }

        // Salva as alterações
        private void SaveNewSettings()
        {
            File.WriteAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YT Downloader\\config.json"), JsonSerializer.Serialize(App.appSettings));
        }
    }
}
