using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using YT_Downloader.Enums;
using YT_Downloader.Models;

namespace YT_Downloader.Services
{
    public class DownloadsService
    {
        private readonly YoutubeService _youtubeService;
        private readonly SemaphoreSlim _semaphore;

        private ConcurrentQueue<DownloadItem> _downloadQueue = new();
        private bool _processing = false;

        public DownloadsService(YoutubeService youtubeService, int maxParallelDownloads = 3)
        {
            _youtubeService = youtubeService;
            _semaphore = new SemaphoreSlim(maxParallelDownloads);
        }

        public void EnqueueDownload(DownloadItem item, CancellationToken token)
        {
            _downloadQueue.Enqueue(item);

            if (!_processing)
            {
                _processing = true;
                _ = ProcessQueueAsync(token);
            }
        }

        private async Task ProcessQueueAsync(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    if (_downloadQueue.TryDequeue(out var item))
                    {
                        _ = DownloadAsync(item);
                    }
                    else
                    {
                        await Task.Delay(200, token);
                        if (_downloadQueue.IsEmpty)
                        {
                            _processing = false;
                            return;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _processing = false;
            }
        }

        private async Task DownloadAsync(DownloadItem item)
        {
            await _semaphore.WaitAsync();
            try
            {
                item.MarkAsDownloading();

                if (item.Type == DownloadType.Video)
                    await _youtubeService.DownloadVideoAsync(
                        item.Manifest, item.VideoStreamOption,
                        item.OutputPath, item.ProgressReporter, item.CTS.Token
                    );
                else
                    await _youtubeService.DownloadAudioAsync(
                        item.Manifest, item.OutputPath, item.ProgressReporter, item.CTS.Token
                    );

                item.MarkAsCompleted();
            }
            catch (OperationCanceledException)
            {
                item.MarkAsCancelled();
            }
            catch (Exception ex)
            {
                item.MarkAsError(ex);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
