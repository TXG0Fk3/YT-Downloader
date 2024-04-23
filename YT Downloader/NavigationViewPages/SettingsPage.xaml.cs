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
            appThemeRadioBt.SelectedIndex = App.appConfig.AppTheme;
            showDefaultPath.Description = App.appConfig.DefaultDownloadsPath;
            askWhereSaveTS.IsOn = App.appConfig.AlwaysAskWhereSave;
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
                        App.appConfig.AppTheme = 0;
                        break;

                    case "Dark": // Escuro
                        rootElement.RequestedTheme = ElementTheme.Dark;
                        App.appConfig.AppTheme = 1;
                        break;

                    case "System": // Padrão do Sistema
                        rootElement.RequestedTheme = ElementTheme.Default;
                        App.appConfig.AppTheme = 2;
                        break;
                }

                // Salva as alterações
                SaveNewConfig();
            }
        }

        // Seleciona a pasta onde serão salvos os downlods
        async private void SelectFolderButton_click(object sender, RoutedEventArgs e)
        {
            FolderPicker openPicker = new();
            nint windowHandle = WindowNative.GetWindowHandle(App.m_window);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, windowHandle);

            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                App.appConfig.DefaultDownloadsPath = folder.Path;
                showDefaultPath.Description = folder.Path;

                // Salva as alterações
                SaveNewConfig();
            }
        }

        // Define se o programa sempre deve perguntar ao usuário onde salvar os downloads,
        // Se estiver desativado, o programa utilizará o diretório padrão
        private void AlwaysAskTB_toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;

            App.appConfig.AlwaysAskWhereSave = toggleSwitch.IsOn;

            // Salva as alterações
            SaveNewConfig();
        }

        // Salva as alterações
        private void SaveNewConfig()
        {
            File.WriteAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YT Downloader\\config.json"), JsonSerializer.Serialize(App.appConfig));
        }


        // Classe que armazena configurações do programa
        public class ConfigFile
        {
            public int AppTheme { get; set; }
            public string DefaultDownloadsPath { get; set; }
            public bool AlwaysAskWhereSave { get; set; }
        }
    }
}
