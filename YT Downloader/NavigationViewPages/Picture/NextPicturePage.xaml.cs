using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;


namespace YT_Downloader.NavigationViewPages.Picture
{
    public sealed partial class NextPicturePage : Page
    {
        // Variáveis estáticas para serem acessadas por outras classes
        public static Frame view;
        public static string url;
        
        public string videoID;
        public string downloadName;

        public NextPicturePage()
        {
            this.InitializeComponent();
            this.Loaded += NextPicturePage_Loaded;
        }

        // Método que é chamado somente quando a page estiver completamente carregada
        private void NextPicturePage_Loaded(object sender, RoutedEventArgs e)
        {
            App.cts = new CancellationTokenSource();
            GetAndShowVideoInfo(App.cts.Token);
        }

        // Coleta informações da URL ou ID do vídeo e mostra ao usuário
        async private void GetAndShowVideoInfo(CancellationToken token)
        {
            try
            {
                var youtube = new YoutubeClient();

                var video = await youtube.Videos.GetAsync(url);
                if (token.IsCancellationRequested) return;

                videoID = video.Id;
                downloadName = video.Title.Replace("\\", "").Replace("<", "").Replace(">", "").Replace(":", "").Replace("*", "").Replace("?", "").Replace("\"", "").Replace("/", "").Replace("|", "");

                pictureResolution.Items.Add(new ComboBoxItem().Content = "maxresdefault");
                pictureResolution.Items.Add(new ComboBoxItem().Content = "hqdefault");
                pictureResolution.Items.Add(new ComboBoxItem().Content = "mqdefault");
                pictureResolution.Items.Add(new ComboBoxItem().Content = "default");

                // Título a ser mostrado pode ter no máximo 60 caracteres
                videoTitle.Text = video.Title.Length > 60 ? $"{video.Title[..60]}..." : video.Title;

                if (token.IsCancellationRequested) return;

                // Habilita o botão de download
                downloadButton.IsEnabled = true;
                pictureResolution.IsEnabled = true;
            }
            catch (Exception ex)
            {
                // Caso o programa tiver algum problema, uma mensagem de erro será mostrada
                ContentDialog dialog = new()
                {
                    XamlRoot = XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "An error has occurred",
                    CloseButtonText = "Close",
                    Content = new NavigationViewPages.ErrorPage(ex.Message)
                };

                _ = await dialog.ShowAsync();
                view.Navigate(typeof(NavigationViewPages.Picture.PicturePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
        }    

        // Caso o usuário altere a Resolução, também altera o tamanho do áudio
        async private void PictureResolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loading.IsActive = true;
            loadingBorder.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            pictureBorder.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

            // Carrega a Thumbnail do vídeo e mostra ao usuário.
            var thumbnailUrl = $"https://img.youtube.com/vi/{videoID}/{pictureResolution.SelectedValue}.jpg";
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(thumbnailUrl);
            var content = await response.Content.ReadAsByteArrayAsync();
            File.WriteAllBytes($"{Path.GetTempPath()}\\{videoID}{pictureResolution.SelectedValue}.jpg", content);
            videoPicture.Source = new BitmapImage(new Uri($"{Path.GetTempPath()}\\{videoID}{pictureResolution.SelectedValue}.jpg"));

            loading.IsActive = false;
            loadingBorder.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            pictureBorder.Visibility = Microsoft.UI.Xaml.Visibility.Visible;

            // Atualiza o tamanho do arquivo
            Run run = new();
            run.Text = $"{Math.Round(new FileInfo($"{Path.GetTempPath()}\\{videoID}{pictureResolution.SelectedValue}.jpg").Length / 1024.0 / 1024.0, 2)} MB";
            pictureSize.Inlines.Clear();
            pictureSize.Inlines.Add(run);

            await Task.Delay(40);
            File.Delete($"{Path.GetTempPath()}\\{videoID}{pictureResolution.SelectedValue}.jpg");
        }

        // Baixa a imagem
        async private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            // Caminho onde será baixado a imagem
            string downloadPath = App.appConfig.DefaultDownloadsPath;
            if (App.appConfig.AlwaysAskWhereSave)
            {
                FolderPicker openPicker = new();
                openPicker.FileTypeFilter.Add("*");

                nint windowHandle = WindowNative.GetWindowHandle(App.m_window);
                WinRT.Interop.InitializeWithWindow.Initialize(openPicker, windowHandle);

                StorageFolder folder = await openPicker.PickSingleFolderAsync();

                if (folder != null) downloadPath = folder.Path;
                // Caso o usuário cancele a escolha da pasta
                else return;
            }
            
            // Baixa a thumbnail
            var thumbnailUrl = $"https://img.youtube.com/vi/{videoID}/{pictureResolution.SelectedValue}.jpg";
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(thumbnailUrl);
            var content = await response.Content.ReadAsByteArrayAsync();
            File.WriteAllBytes($"{downloadPath}\\{downloadName}.jpg", content);

            // Envia os dados para DownloadFinishedPage.
            NavigationViewPages.DownloadFinishedPage.view = view;
            NavigationViewPages.DownloadFinishedPage.downloadPath = downloadPath;
            NavigationViewPages.DownloadFinishedPage.vidTitle = videoTitle.Text;
            NavigationViewPages.DownloadFinishedPage.downloadType = "P";
            view.Navigate(typeof(NavigationViewPages.DownloadFinishedPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        // Cancela a operação
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.cts.Cancel();
            view.Navigate(typeof(NavigationViewPages.Picture.PicturePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
    }
}