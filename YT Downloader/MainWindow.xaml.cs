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
using AngleSharp.Dom;
using System.Threading;


namespace YT_Downloader
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            // Carrega o tema escolhido pelo usuário
            switch (App.appConfig.AppTheme)
            {
                case 0:
                    rootElement.RequestedTheme = ElementTheme.Light; // Claro
                    break;

                case 1:
                    rootElement.RequestedTheme = ElementTheme.Dark; // Escuro
                    break;

                case 2:
                    rootElement.RequestedTheme = ElementTheme.Default; // Padrão do Sistema
                    break;
            }

            // Tamanho da Janela e Título "infinito"
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(660, 410));
            this.ExtendsContentIntoTitleBar = true;

            // Passa o Frame (view) para todas as pages e incializa a VideoPage (Padrão)
            NavigationViewPages.Video.VideoPage.view = view;
            NavigationViewPages.Music.MusicPage.view = view;
            NavigationViewPages.Picture.PicturePage.view = view;
            NavigationViewPages.SettingsPage.rootElement = rootElement;
            view.Navigate(typeof(NavigationViewPages.Video.VideoPage), null);
        }

        // Método que altera a page sendo mostrada
        public void NavigationViewSwitch(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            App.cts.Cancel(); // Para cancelar operações que podem estar sendo executadas
            if (args.IsSettingsInvoked)
            {
                view.Navigate(typeof(NavigationViewPages.SettingsPage), null); // Settings
            }
            else
            {
                var tag = args.InvokedItemContainer.Tag.ToString();
                switch (tag)
                {
                    case "vid": // Video
                        view.Navigate(typeof(NavigationViewPages.Video.VideoPage), null);
                        break;

                    case "mus": // Music
                        view.Navigate(typeof(NavigationViewPages.Music.MusicPage), null);
                        break;

                    case "pic": // Picture
                        view.Navigate(typeof(NavigationViewPages.Picture.PicturePage), null);
                        break;
                }
            }
        }
    }
}
