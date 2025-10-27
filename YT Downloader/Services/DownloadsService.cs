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
        private bool _processing;

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
            while (_downloadQueue.TryDequeue(out var item))
            {
                await _semaphore.WaitAsync(token);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        item.MarkAsDownloading();
                        if (item.Type == DownloadType.Video)
                            await _youtubeService.DownloadVideoAsync(
                                item.VideoStreamInfo, item.AudioStreamInfo,
                                item.OutputPath, new Progress<double>(item.UpdateProgress), token
                            );
                        else
                            await _youtubeService.DownloadAudioAsync(
                                item.AudioStreamInfo,
                                item.OutputPath, new Progress<double>(item.UpdateProgress), token
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
                }, token);
            }

            _processing = false;
        }
    }
}
