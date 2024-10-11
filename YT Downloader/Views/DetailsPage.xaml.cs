using AngleSharp.Dom;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;


namespace YT_Downloader.Views
{
    public sealed partial class DetailsPage : Page
    {
        private readonly YoutubeClient YoutubeClient;
        private YoutubeExplode.Playlists.Playlist Playlist;
        private YoutubeExplode.Videos.Video Video;
        private StreamManifest StreamManifest;
        private string ThumbnailPath;
        private CancellationToken token;

        private ContentDialog ContentDialogInstance;

        public DetailsPage(ContentDialog contentDialogInstance)
        {
            YoutubeClient = new();
            ContentDialogInstance = contentDialogInstance;
            ContentDialogInstance.PrimaryButtonClick += DownloadButton_Clicked;

            InitializeComponent();

            ResetVideoInfo();
        }

        // Ação ao apertar Enter no UrlTextBox
        private void UrlBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ResetVideoInfo();
                LoadVideoInfoAsync();
            }
        }

        // Ação ao apertar no LoadInfoButton
        public void LoadInfoButton_Clicked(object sender, RoutedEventArgs e)
        {
            ResetVideoInfo();
            LoadVideoInfoAsync();
        }
        
        // Carrega informações do vídeo
        private async void LoadVideoInfoAsync()
        {
            try
            {
                VideoInfoGrid.Visibility = Microsoft.UI.Xaml.Visibility.Visible;

                Video = await YoutubeClient.Videos.GetAsync(UrlTextBox.Text);
                if (token.IsCancellationRequested) return;

                StreamManifest = await YoutubeClient.Videos.Streams.GetManifestAsync(UrlTextBox.Text);
                if (token.IsCancellationRequested) return;

                DisplayVideoInfo();
            }
            catch (Exception ex)
            {
                //await ShowErrorDialogAsync("An error occurred while loading the video.", ex);
                //App.mainWindow.NavigateToPreviousPage(typeof(VideoPage));
            }
        }

        // Remove informações do vídeo
        private void ResetVideoInfo()
        {
            ContentDialogInstance.IsPrimaryButtonEnabled = false;

            TitleHyperlink.Content = "Loading...";
            TitleHyperlink.NavigateUri = null;

            FormatComboBox.IsEnabled = false;

            QualityComboBox.IsEnabled = false;
            QualityComboBox.Items.Clear();

            FileNameTextBox.IsEnabled = false;
            FileNameTextBox.PlaceholderText = "Loading...";

            FileSize.Text = "Loading...";

            ThumbnailBorder.Child = new ProgressRing
            {
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
            };
        }

        // Mostra informações do vídeo
        private async void DisplayVideoInfo()
        {
            TitleHyperlink.Content = Video.Title;
            TitleHyperlink.NavigateUri = new Uri(Video.Url);
            FileNameTextBox.PlaceholderText = string.Concat(Video.Title.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));

            if (FormatComboBox.SelectedIndex != 0)
                FormatComboBox.SelectedIndex = 0;
            else UpdateQualityComboBox();

            ThumbnailBorder.Child = new Image
            {
                Source = new BitmapImage(new Uri(ThumbnailPath = await Utils.ThumbHelper.DownloadThumbnailAsync(Video.Id))),
                Stretch = Microsoft.UI.Xaml.Media.Stretch.Fill
            };

            FormatComboBox.IsEnabled = true;
            QualityComboBox.IsEnabled = true;
            FileNameTextBox.IsEnabled = true;

            ContentDialogInstance.IsPrimaryButtonEnabled = true;
        }

        // Ação caso a seleção do formato seja alterada
        private void Format_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateQualityComboBox();
            QualityComboBox.IsEnabled = FormatComboBox.SelectedItem.ToString() == "Mp4";
        }
            
        // Ação caso a seleção da qualidade seja alterada
        private void Quality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (QualityComboBox.SelectedItem != null) FileSize.Text = $"{Math.Round(Size, 2)} MB";
        }

        // Atualiza as qualidades disponíveis em QualityComboBox
        private void UpdateQualityComboBox()
        {
            QualityComboBox.Items.Clear();
            foreach (var label in Qualities)
                QualityComboBox.Items.Add(new ComboBoxItem().Content = label);
            QualityComboBox.SelectedIndex = 0;
        }

        // Ação a ser executada caso o botão Download do ContentDialog seja pressionado
        internal async void DownloadButton_Clicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string downloadPath = await GetDownloadPathAsync();
            if (string.IsNullOrEmpty(downloadPath)) return;

            App.mainWindow.AddDownloadToStack(new Controls.DownloadCard(YoutubeClient, downloadPath, ThumbnailPath, FileName, Video, SelectedAudioStreamInfo, SelectedVideoStreamInfo));
        }

        // Coleta o caminho padrão para salvar arquivos ou chama o filepicker
        private async Task<string> GetDownloadPathAsync()
        {
            if (App.appSettings.AlwaysAskWhereSave)
            {
                var openPicker = new FolderPicker
                {
                    FileTypeFilter = { "*" }
                };

                InitializeWithWindow.Initialize(openPicker, WindowNative.GetWindowHandle(App.mainWindow));

                StorageFolder folder = await openPicker.PickSingleFolderAsync();
                return folder?.Path;
            }

            return App.appSettings.DefaultDownloadsPath;
        }

        // Video selecionado
        private VideoOnlyStreamInfo SelectedVideoStreamInfo
        {
            get
            {
                int selectedFps = QualityComboBox.SelectedValue.ToString().Split(' ')[0].EndsWith("60") ? 60 : 30;
                int selectedResolution = int.Parse(QualityComboBox.SelectedValue.ToString().Split('p')[0]);

                return StreamManifest.GetVideoOnlyStreams()
                    .Where(s => s.Container == Container.Mp4)
                    .OrderBy(s => Math.Abs((int.Parse(s.VideoQuality.Label.Split('p')[0]) - selectedResolution)) 
                                + Math.Abs((s.VideoQuality.Label.Split(' ')[0].EndsWith("60") ? 60 : 30) - selectedFps))
                    .FirstOrDefault();
            }
        }

        // Áudio selecionado
        private AudioOnlyStreamInfo SelectedAudioStreamInfo
        {
            get
            {
                return (AudioOnlyStreamInfo)StreamManifest.GetAudioOnlyStreams()
                    .Where(s => s.Container == Container.Mp4)
                    .GetWithHighestBitrate();
            }
        }

        // Nome do arquivo
        private string FileName
        {
            get
            {
                return FileNameTextBox.Text == ""
                    ? FileNameTextBox.PlaceholderText
                    : FileNameTextBox.Text;
            }
        }

        // Qualidades disponíveis
        private HashSet<string> Qualities
        {
            get
            {
                return FormatComboBox.SelectedItem.ToString() == "Mp4"
                    ? StreamManifest
                      .GetVideoOnlyStreams()
                      .Where(s => s.Container == Container.Mp4)
                      .Select(s => s.VideoQuality.Label)
                      .ToHashSet()
                    : ["Best"];
            }
        }

        // Tamanho do arquivo (não será exibido em playlists)
        private double Size
        {
            get
            {
                return FormatComboBox.SelectedItem.ToString() == "Mp4"
                    ? SelectedVideoStreamInfo.Size.MegaBytes + SelectedAudioStreamInfo.Size.MegaBytes
                    : SelectedAudioStreamInfo.Size.MegaBytes;
            }
        }
    }
}
