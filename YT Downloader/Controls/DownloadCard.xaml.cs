using CommunityToolkit.WinUI;
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
        private readonly Video Video;
        private readonly VideoOnlyStreamInfo VideoStreamInfo;
        private readonly AudioOnlyStreamInfo AudioStreamInfo;
        public CancellationToken token;

        public DownloadCard(YoutubeClient youtubeClient,
                            string downloadPath,
                            string thumbnailPath,
                            string fileName,
                            Video video,
                            AudioOnlyStreamInfo audioStreamInfo,
                            VideoOnlyStreamInfo videoStreamInfo = null)
        {
            YoutubeClient = youtubeClient;
            DownloadPath = downloadPath;
            ThumbnailPath = thumbnailPath;
            FileName = string.Concat(fileName.Where(c => !Path.GetInvalidFileNameChars().Contains(c))); // Remove caracteres inválidos
            Video = video;
            VideoStreamInfo = videoStreamInfo;
            AudioStreamInfo = audioStreamInfo;
            token = new();

            InitializeComponent();

            LoadVideoInfo();

            // Inicia o download de forma assíncrona
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
            try
            {
                await (VideoStreamInfo != null
                    ? DownloadVideoAsync()
                    : DownloadAudioAsync()).ConfigureAwait(false);

                // Se chegou até aqui, significa que o Download foi feito com sucesso e atualiza o progrebar para 100%
                DispatcherQueue.TryEnqueue(() => DownloadProgressBar.Value = 100);
            }
            catch (Exception ex)
            {
                // Atualiza a UI em caso de erro
                await DispatcherQueue.EnqueueAsync(() => DownloadProgressBar.ShowError = true);

                Debug.WriteLine(ex);
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
                        DispatcherQueue.TryEnqueue(() => DownloadProgressBar.Value = p * 100);
                }), token);
        }

        private async Task DownloadAudioAsync()
        {
            // Faz o download apenas do áudio e atualiza o progresso
            await YoutubeClient.Videos.Streams.DownloadAsync(AudioStreamInfo, $"{DownloadPath}\\{FileName}.mp3",
                new Progress<double>(p =>
                {
                    // Atualiza a UI apenas se a diferença de progresso for maior que 1% (ou outro valor que fizer sentido)
                    if (Math.Abs(p % 0.01) < 0.0001 || p == 1.0)
                        DispatcherQueue.TryEnqueue(() => DownloadProgressBar.Value = p * 100);
                }), token);
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
