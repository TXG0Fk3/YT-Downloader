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


namespace YT_Downloader.NavigationViewPages
{
    public sealed partial class SettingsPage : Page
    {
        public static Grid rootElement;

        public SettingsPage()
        {
            this.InitializeComponent();
            this.showDefaultPath.Description = @"C:\users\leove\Download\YT_Downloader";
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
                        break;

                    case "Dark":
                        rootElement.RequestedTheme = ElementTheme.Dark;
                        break;

                    case "System":
                        rootElement.RequestedTheme = ElementTheme.Default;
                        break;
                }
            }
        }
    }
}
