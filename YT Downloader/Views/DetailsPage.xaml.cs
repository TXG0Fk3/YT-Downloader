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
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;


namespace YT_Downloader.Views
{
    public sealed partial class DetailsPage : Page
    {
        private readonly YoutubeClient YoutubeClient;
        private YoutubeExplode.Playlists.Playlist Playlist;
        private Video Video;
        private StreamManifest StreamManifest;
        private string ThumbnailPath;
        private CancellationTokenSource CTS;

        private ContentDialog ContentDialogInstance;

        public DetailsPage(ContentDialog contentDialogInstance)
        {
            YoutubeClient = new();
            CTS = new();
            ContentDialogInstance = contentDialogInstance;
            ContentDialogInstance.PrimaryButtonClick += DownloadButton_Clicked;
            ContentDialogInstance.CloseButtonClick += CancelButton_Clicked;

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

                if (UrlTextBox.Text.Contains("playlist?list=")) //Caso seja uma playlist
                    Playlist = await YoutubeClient.Playlists.GetAsync(UrlTextBox.Text, CTS.Token);
                else //Caso seja um único vídeo/música
                {
                    Video = await YoutubeClient.Videos.GetAsync(UrlTextBox.Text, CTS.Token);
                    StreamManifest = await YoutubeClient.Videos.Streams.GetManifestAsync(UrlTextBox.Text, CTS.Token);
                }

                DisplayVideoInfo();
            }
            catch (Exception ex)
            {
                VideoInfoGrid.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                ResetVideoInfo();

                ErrorInfoBar.Severity = InfoBarSeverity.Error;
                ErrorInfoBar.Title = "Error";
                ErrorInfoBar.Message = ex.Message;
                ErrorInfoBar.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }
        }

        // Remove informações do vídeo
        private void ResetVideoInfo()
        {
            ContentDialogInstance.IsPrimaryButtonEnabled = false;

            Video = null;
            Playlist = null;

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

            ErrorInfoBar.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }

        // Mostra informações do vídeo
        private async void DisplayVideoInfo()
        {
            if (Playlist != null) // Caso for uma playlist
            {
                TitleHyperlink.Content = Playlist.Title;
                TitleHyperlink.NavigateUri = new Uri(Playlist.Url);

                FileNameTextBox.PlaceholderText = "Custom FileName not available for Playlists.";

                ThumbnailBorder.Child = new FontIcon
                {
                    Glyph = "\uF140",
                    FontSize = 32,
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                    VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
                };
            }
            else // Caso seja um único vídeo/música 
            {
                TitleHyperlink.Content = Video.Title;
                TitleHyperlink.NavigateUri = new Uri(Video.Url);
                FileNameTextBox.PlaceholderText = string.Concat(Video.Title.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));

                ThumbnailBorder.Child = new Image
                {
                    Source = new BitmapImage(new Uri(ThumbnailPath = await Utils.ThumbHelper.DownloadThumbnailAsync(Video.Id))),
                    Stretch = Microsoft.UI.Xaml.Media.Stretch.Fill
                };

                FileNameTextBox.IsEnabled = true;
            }

            if (FormatComboBox.SelectedIndex != 0)
                FormatComboBox.SelectedIndex = 0;
            else UpdateQualityComboBox();

            FormatComboBox.IsEnabled = true;
            QualityComboBox.IsEnabled = true;         

            ContentDialogInstance.IsPrimaryButtonEnabled = true;
        }

        // Ação caso a seleção do formato seja alterada
        private void Format_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateQualityComboBox();
            QualityComboBox.IsEnabled = FormatComboBox.SelectedItem.ToString() == "Mp4";
        }

        // Ação caso a seleção da qualidade seja alterada
        private void Quality_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            FileSize.Text = (QualityComboBox.SelectedItem != null && Video != null) ? $"{Math.Round(Size, 2)} MB" : "Undetermined";

        // Atualiza as qualidades disponíveis em QualityComboBox
        private void UpdateQualityComboBox()
        {
            QualityComboBox.Items.Clear();
            foreach (var label in Qualities)
                QualityComboBox.Items.Add(new ComboBoxItem().Content = label);
            QualityComboBox.SelectedIndex = 0;
        }

        // Ação a ser executada caso o botão Download do ContentDialog seja pressionado
        private async void DownloadButton_Clicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string downloadPath = await GetDownloadPathAsync();
            if (string.IsNullOrEmpty(downloadPath)) return;

            if (Playlist != null) // Caso for uma playlist
            {
                await foreach (var video in YoutubeClient.Playlists.GetVideosAsync(Playlist.Id))
                {
                    StreamManifest = await YoutubeClient.Videos.Streams.GetManifestAsync(video.Url, CTS.Token);
                    ThumbnailPath = await Utils.ThumbHelper.DownloadThumbnailAsync(video.Id);
                    
                    App.mainWindow.AddDownloadToStack(new Controls.DownloadCard(
                        YoutubeClient, downloadPath, ThumbnailPath, video.Title,
                        video, SelectedVideoStreamInfo, SelectedAudioStreamInfo));
                }
            }
            else // Caso for um único vídeo/música
            {
                App.mainWindow.AddDownloadToStack(new Controls.DownloadCard(
                    YoutubeClient, downloadPath, ThumbnailPath, FileName,
                    Video, SelectedVideoStreamInfo, SelectedAudioStreamInfo));
            }
        }

        // Cancela o processo e fecha o contentdialog
        private void CancelButton_Clicked(ContentDialog sender, ContentDialogButtonClickEventArgs args) =>
            CTS.Cancel();

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
                if (FormatComboBox.SelectedItem.ToString() == "Mp4")
                {
                    int selectedFps = QualityComboBox.SelectedValue.ToString().Split(' ')[0].EndsWith("60") ? 60 : 30;
                    int selectedResolution = int.Parse(QualityComboBox.SelectedValue.ToString().Split('p')[0]);

                    return StreamManifest.GetVideoOnlyStreams()
                        .Where(s => s.Container == Container.Mp4)
                        .OrderBy(s => Math.Abs((int.Parse(s.VideoQuality.Label.Split('p')[0]) - selectedResolution))
                                    + Math.Abs((s.VideoQuality.Label.Split(' ')[0].EndsWith("60") ? 60 : 30) - selectedFps))
                        .FirstOrDefault();
                }
                else
                    return null;
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
                if (Playlist != null) // Caso for playlist
                    return null;
                else
                    return FileNameTextBox.Text == ""
                        ? Video.Title
                        : FileNameTextBox.Text;
            }
        }

        // Qualidades disponíveis
        private HashSet<string> Qualities
        {
            get
            {
                if (FormatComboBox.SelectedItem.ToString() == "Mp4")
                    return Playlist != null
                        ? ["2160p60", "2160p", "1440p60", "1440p", "1080p60", "1080p", "720p60", "720p", "480p60", "480p", "360p60", "360p", "240p60", "240p", "144p60", "144p"]
                        : StreamManifest
                            .GetVideoOnlyStreams()
                            .Where(s => s.Container == Container.Mp4)
                            .Select(s => s.VideoQuality.Label)
                            .ToHashSet();
                else
                    return ["Best"];
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
