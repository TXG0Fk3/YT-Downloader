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
using Microsoft.UI.Xaml.Media.Animation;


namespace YT_Downloader.NavigationViewPages.Video
{
    public sealed partial class VideoPage : Page
    {
        public static Frame view;

        public VideoPage()
        {
            this.InitializeComponent();
        }

        private void NextButtonClicked(object sender, RoutedEventArgs e)
        {
            NavigationViewPages.Video.NextVideoPage.url = urlbox.Text;
            NavigationViewPages.Video.NextVideoPage.view = view;
            view.Navigate(typeof(NavigationViewPages.Video.NextVideoPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
