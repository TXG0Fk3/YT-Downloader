using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YT_Downloader.Controls
{
    public sealed partial class DownloadCard : UserControl
    {
        private readonly YoutubeClient YoutubeClient;
        private readonly string DownloadPath;
        private readonly string FileName;
        private readonly string ThumbnailPath;
        private readonly IVideo Video;
        private readonly VideoOnlyStreamInfo VideoStreamInfo;
        private readonly AudioOnlyStreamInfo AudioStreamInfo;
        private CancellationTokenSource CTS;

        private Exception Ex;

        public event EventHandler DownloadCompleted;

        public DownloadCard(YoutubeClient youtubeClient,
                            string downloadPath,
                            string thumbnailPath,
                            string fileName,
                            IVideo video,
                            VideoOnlyStreamInfo videoStreamInfo,
                            AudioOnlyStreamInfo audioStreamInfo,
                            CancellationTokenSource playlistCTS = null)
        {
            YoutubeClient = youtubeClient;
            DownloadPath = downloadPath;
            ThumbnailPath = thumbnailPath;
            FileName = string.Concat(fileName.Where(c => !Path.GetInvalidFileNameChars().Contains(c))); // Remove caracteres inválidos
            Video = video;
            VideoStreamInfo = videoStreamInfo;
            AudioStreamInfo = audioStreamInfo;
            CTS = new();

            playlistCTS?.Token.Register(() => CTS.Cancel());

            InitializeComponent();

            LoadVideoInfo();

            // Inicia o download
            _ = DownloadAsync();
        }

        private void LoadVideoInfo()
        {
            // Carrega as informações do vídeo na UI
            TitleHyperlink.Content = Video.Title;
            TitleHyperlink.NavigateUri = new Uri(Video.Url);
            ChannelTextBlock.Text = Video.Author.ToString();
            VideoQualityAndSizeTextBlock.Text = $"{DownloadQuality} {Math.Round(DownloadSize, 2)} MB";

            ThumbnailBorder.Child = new Image
            {
                Source = new BitmapImage(new Uri(ThumbnailPath)),
                Stretch = Microsoft.UI.Xaml.Media.Stretch.Fill
            };
        }

        private async Task DownloadAsync()
        {
            DispatcherQueue.TryEnqueue(() => 
            {
                ErrorInfoBar.Visibility = Visibility.Collapsed;
                DownloadProgressBar.ShowError = false;
            });

            Button1.Content = new FontIcon { Glyph = "\uE8DA" };
            Button1ToolTip.Content = "Open Local";
            Button1.IsEnabled = false;

            Button2.Content = new FontIcon { Glyph = "\uF78A" };
            Button2ToolTip.Content = "Cancel";
            Button2.Click += CancelButton_Click;

            try
            {
                await (VideoStreamInfo != null
                    ? DownloadVideoAsync()
                    : DownloadAudioAsync()).ConfigureAwait(false);

                // Se chegou até aqui, significa que o Download foi feito com sucesso e atualiza o progressbar para 100%
                DispatcherQueue.TryEnqueue(() =>
                {
                    DownloadProgressBar.Value = 100;
                    DownloadProgressPercent.Text = $"100%";
                    Button1.IsEnabled = true;
                    Button1.Click += OpenLocalButton_Click;

                    Button2.Content = new FontIcon { Glyph = "\uE74D" };
                    Button2ToolTip.Content = "Delete";
                    Button2.Click -= CancelButton_Click;
                    Button2.Click += DeleteButton_Click;

                    DownloadCompleted?.Invoke(this, EventArgs.Empty); // Informa a todos os observadores que o Download foi concluído
                });
            }
            catch (Exception ex)
            {
                Ex = ex;
                // Atualiza a UI em caso de erro
                DispatcherQueue.TryEnqueue(() =>
                {
                    DownloadProgressBar.ShowError = true;
                    ErrorInfoBar.Visibility = Visibility.Visible;

                    Button1.Content = new FontIcon { Glyph = "\uE72C" };
                    Button1ToolTip.Content = "Retry";
                    Button1.Click += RepeatButton_Click;
                    Button1.IsEnabled = true;
                });
            }
        }

        private async Task DownloadVideoAsync()
        {
            var streamInfos = new IStreamInfo[] { AudioStreamInfo, VideoStreamInfo };
            // Faz o download do vídeo e atualiza o progresso
            await YoutubeClient.Videos.DownloadAsync(streamInfos,
                new ConversionRequestBuilder($"{DownloadPath}\\{FileName}.mp4").Build(),
                new Progress<double>(p =>
                {
                    // Atualiza a UI apenas se a diferença de progresso for maior que 1% (ou outro valor que fizer sentido)
                    if (Math.Abs(p % 0.01) < 0.0001 || p == 1.0)
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            DownloadProgressBar.Value = p * 100;
                            DownloadProgressPercent.Text = $"{p * 100:00}%";
                        });
                }), CTS.Token);
        }

        private async Task DownloadAudioAsync()
        {
            // Faz o download apenas do áudio e atualiza o progresso
            await YoutubeClient.Videos.Streams.DownloadAsync(AudioStreamInfo, $"{DownloadPath}\\{FileName}.mp3",
                new Progress<double>(p =>
                {
                    // Atualiza a UI apenas se a diferença de progresso for maior que 1% (ou outro valor que fizer sentido)
                    if (Math.Abs(p % 0.01) < 0.0001 || p == 1.0)
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            DownloadProgressBar.Value = p * 100;
                            DownloadProgressPercent.Text = $"{p * 100:00}%";
                        });
                }), CTS.Token);
        }

        private void OpenLocalButton_Click(object sender, RoutedEventArgs e) =>
            Process.Start("explorer.exe", DownloadPath);

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CTS.Cancel();
            (Parent as StackPanel).Children.Remove(this);
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            Button1.Click -= RepeatButton_Click;
            _ = DownloadAsync();
        }    

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (VideoStreamInfo != null)
                File.Delete($"{DownloadPath}\\{FileName}.mp4");
            else
                File.Delete($"{DownloadPath}\\{FileName}.mp3");

            (Parent as StackPanel).Children.Remove(this);
        }

        // Exibe um diálogo de erro
        private async void ErrorButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "An error occurred while downloading the video.",
                Content = new Views.ErrorPage(Ex.Message),
                CloseButtonText = "Close"
            };

            await dialog.ShowAsync();
        }

        // Propriedade que retorna a qualidade do download (vídeo ou áudio)
        private string DownloadQuality
        {
            get
            {
                return VideoStreamInfo != null
                    ? VideoStreamInfo.VideoQuality.Label
                    : AudioStreamInfo.Bitrate.ToString();
            }
        }

        // Propriedade que retorna o tamanho total do download
        private double DownloadSize
        {
            get
            {
                return VideoStreamInfo != null
                    ? VideoStreamInfo.Size.MegaBytes + AudioStreamInfo.Size.MegaBytes
                    : AudioStreamInfo.Size.MegaBytes;
            }
        }
    }
}
