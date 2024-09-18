using AngleSharp.Dom;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using YT_Downloader.Utils;

namespace YT_Downloader.Views.Video
{
    public sealed partial class NextVideoPage : Page
    {
        private YoutubeVideoHelper YoutubeHelper;


        public NextVideoPage() => InitializeComponent();

        // M�todo chamado sempre que a navega��o para esta p�gina ocorre
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Coleta informa��es da URL ou ID do v�deo e mostra ao usu�rio
            try
            {
                YoutubeHelper = await YoutubeVideoHelper.CreateAsync(e.Parameter as string);

                DisplayVideoInfo();
                EnableDownloadButton();
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync("An error occurred while loading the video.", ex);
                App.MainWindow.NavigateToPreviousPage(typeof(VideoPage));
            }
        }

        // Exibe as informa��es do v�deo na UI
        private void DisplayVideoInfo()
        {
            var videoTitle = YoutubeHelper.GetTitle();
            videoTitleTBC.Text = videoTitle.Length > 60 ? $"{videoTitle[..60]}..." : videoTitle;
            videoResolutionCB.Items.Clear();

            foreach (var resolution in YoutubeHelper.GetStreamResolutions()) videoResolutionCB.Items.Add(new ComboBoxItem().Content = resolution);

            LoadVideoThumbnail();
        }

        // Carrega e exibe a miniatura do v�deo
        private async void LoadVideoThumbnail()
        {
            pictureBorder.Child = new Image
            {
                Source = new BitmapImage(new Uri(await YoutubeHelper.GetThumbnailAsync())),
                Width = 460,
                Height = 260,
                Stretch = Microsoft.UI.Xaml.Media.Stretch.Fill
            };
        }

        // Habilita o bot�o de download
        private void EnableDownloadButton()
        {
            downloadButton.IsEnabled = true;
            videoResolutionCB.IsEnabled = true;
        }

        // Atualiza o tamanho do v�deo baseado na resolu��o escolhida
        private void VideoResolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            videoSize.Inlines.Clear();
            videoSize.Inlines.Add(new Run {Text = $"{Math.Round(YoutubeHelper.GetSize(videoResolutionCB.SelectedValue.ToString()), 2)} MB" });
        }

        // Inicia o download do v�deo
        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string downloadPath = await GetDownloadPathAsync();
            if (string.IsNullOrEmpty(downloadPath)) return;

            var parameters = new
            {
                DownloadPath = downloadPath,
                YoutubeHelper,
                SelectedResolution = videoResolutionCB.SelectedValue.ToString()
            };

            App.MainWindow.NavigateToNextPage(typeof(DownloadPage), parameters);
        }

        // Obt�m o caminho de download escolhido pelo usu�rio
        private async Task<string> GetDownloadPathAsync()
        {
            if (App.AppSettings.AlwaysAskWhereSave)
            {
                var openPicker = new FolderPicker
                {
                    FileTypeFilter = { "*" }
                };

                InitializeWithWindow.Initialize(openPicker, WindowNative.GetWindowHandle(App.MainWindow));

                StorageFolder folder = await openPicker.PickSingleFolderAsync();
                return folder?.Path;
            }

            return App.AppSettings.DefaultDownloadsPath;
        }

        // Exibe um di�logo de erro
        private async Task ShowErrorDialogAsync(string title, Exception ex)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = title,
                Content = new Views.ErrorPage(ex.Message),
                CloseButtonText = "Close"
            };

            await dialog.ShowAsync();
        }

        // Cancela a opera��o de carregamento ou download
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.Cts.Cancel();
            App.MainWindow.NavigateToPreviousPage(typeof(VideoPage));
        }
    }
}
