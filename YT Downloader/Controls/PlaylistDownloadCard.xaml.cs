using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace YT_Downloader.Controls
{
    public sealed partial class PlaylistDownloadCard : UserControl
    {
        private readonly string PlaylistTitle;
        private readonly string PlaylistURL;
        private readonly string Author;
        private readonly string DownloadQuality;
        private readonly string FolderPath;
        public int? PlaylistVideoCount;
        private int CompletedDownloadsCount;
        private CancellationTokenSource CTS;

        public PlaylistDownloadCard(string playlistTitle,
                                    string playlistURL,
                                    string author,
                                    string downloadQuality,
                                    string folderPath,
                                    int? playlistVideoCount,
                                    CancellationTokenSource cts)
        {
            PlaylistTitle = playlistTitle;
            PlaylistURL = playlistURL;
            Author = author;
            DownloadQuality = downloadQuality;
            FolderPath = folderPath;
            PlaylistVideoCount = playlistVideoCount;
            CompletedDownloadsCount = 0;
            CTS = cts;

            InitializeComponent();

            LoadPlaylistInfo();
        }

        private void LoadPlaylistInfo()
        {
            // Carrega as informações da Playlist na UI
            TitleHyperlink.Content = PlaylistTitle;
            TitleHyperlink.NavigateUri = new Uri(PlaylistURL);
            ChannelTextBlock.Text = Author;
            VideoQuality.Text = $"{DownloadQuality} - {CompletedDownloadsCount} Downloads Completed";
        }

        public void AddDownloadCardToStack(DownloadCard downloadCard)
        {
            DownloadsStackPanel.Children.Add(downloadCard);
        }

        public void DownloadCard_DownloadCompleted(object sender = null, EventArgs e = null)
        {
            CompletedDownloadsCount++;
            UpdateDownloadProgress();
        }

        public void UpdateDownloadProgress()
        {
            double downloadPercentage = (CompletedDownloadsCount / (double)PlaylistVideoCount) * 100;

            DispatcherQueue.TryEnqueue(() =>
            {
                VideoQuality.Text = $"{DownloadQuality} - {CompletedDownloadsCount} Downloads Completed";
                DownloadProgressBar.Value = downloadPercentage;
                DownloadProgressPercent.Text = $"{downloadPercentage:00}%";
            });
        }

        private void OpenLocalButton_Click(object sender, RoutedEventArgs e) =>
            Process.Start("explorer.exe", FolderPath);

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            CTS.Cancel();

            (Parent as StackPanel).Children.Remove(this);

            await Task.Delay(1000);

            if (Directory.Exists(FolderPath))
            {
                Directory.Delete(FolderPath, true);
            }
        }
    }
}
