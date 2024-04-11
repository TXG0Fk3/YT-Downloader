using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using CommunityToolkit.WinUI.Controls.SettingsControlsRns;
using System.Diagnostics;
using Microsoft.UI;
using System.Text.Json;
using Windows.Storage.Pickers;
using Windows.Storage.AccessCache;
using Windows.Storage;


namespace YT_Downloader.NavigationViewPages
{
    public sealed partial class SettingsPage : Page
    {
        public static Grid rootElement;

        public SettingsPage()
        {
            this.InitializeComponent();
            appThemeRadioBt.SelectedIndex = App.appConfig.AppTheme;
            showDefaultPath.Description = App.appConfig.DefaultDownloadsPath;
            askWhereSaveTS.IsOn = App.appConfig.AlwaysAskWhereSave;
        }

        private void Theme_SelectionChanged(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (sender as RadioButtons).SelectedItem as RadioButton;
            if (radioButton != null)
            {
                string theme = radioButton.Content.ToString();
                switch (theme)
                {
                    case "Light":
                        rootElement.RequestedTheme = ElementTheme.Light;
                        App.appConfig.AppTheme = 0;
                        break;

                    case "Dark":
                        rootElement.RequestedTheme = ElementTheme.Dark;
                        App.appConfig.AppTheme = 1;
                        break;

                    case "System":
                        rootElement.RequestedTheme = ElementTheme.Default;
                        App.appConfig.AppTheme = 2;
                        break;
                }

                SaveNewConfig();
            }
        }

        async private void SelectFolderButton_click(object sender, RoutedEventArgs e)
        {
            FolderPicker openPicker = new();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.m_window);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;

            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                App.appConfig.DefaultDownloadsPath = folder.Path;
                showDefaultPath.Description = folder.Path;
                SaveNewConfig();
            }
        }

        private void AlwaysAskTB_toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;

            App.appConfig.AlwaysAskWhereSave = toggleSwitch.IsOn;

            SaveNewConfig();
        }

        private void SaveNewConfig()
        {
            System.IO.File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"), JsonSerializer.Serialize(App.appConfig));
        }
    }

    public class ConfigFile
    {
        public int AppTheme { get; set; }
        public string DefaultDownloadsPath { get; set; }
        public bool AlwaysAskWhereSave { get; set; }
    }
}
