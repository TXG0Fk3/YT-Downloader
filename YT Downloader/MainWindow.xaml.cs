using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI;
using System.Formats.Asn1;
using Microsoft.UI.Xaml.Media.Animation;
using System.Runtime.CompilerServices;


namespace YT_Downloader
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            // Window
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(660, 410));

            // TittleBar
            this.ExtendsContentIntoTitleBar = true;

            // NavigationViewInit
            NavigationViewPages.Video.VideoPage.view = view;
            NavigationViewPages.Music.MusicPage.view = view;
            NavigationViewPages.Picture.PicturePage.view = view;
            view.Navigate(typeof(NavigationViewPages.Video.VideoPage), null);
        }

        public void NavigationViewSwitch(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                view.Navigate(typeof(NavigationViewPages.SettingsPage), null);
            }
            else
            {
                var tag = args.InvokedItemContainer.Tag.ToString();
                switch (tag)
                {
                    case "vid":
                        view.Navigate(typeof(NavigationViewPages.Video.VideoPage), null);
                        break;

                    case "mus":
                        view.Navigate(typeof(NavigationViewPages.Music.MusicPage), null);
                        break;

                    case "pic":
                        view.Navigate(typeof(NavigationViewPages.Picture.PicturePage), null);
                        break;
                }
            }
        }
    }
}
