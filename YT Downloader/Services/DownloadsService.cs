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

        private ConcurrentQueue<DownloadTask> _downloadQueue = new();
        private bool _processing = false;

        public DownloadsService(YoutubeService youtubeService, int maxParallelDownloads = 3)
        {
            _youtubeService = youtubeService;
            _semaphore = new SemaphoreSlim(maxParallelDownloads);
        }

        public void EnqueueDownload(DownloadTask task, CancellationToken token)
        {
            _downloadQueue.Enqueue(task);

            if (!_processing)
            {
                _processing = true;
                _ = ProcessQueueAsync(token);
            }
        }

        private async Task ProcessQueueAsync(CancellationToken token)
        {
            while (_downloadQueue.TryDequeue(out var task))
            {
                await _semaphore.WaitAsync(token);

                var item = task.Item;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        item.MarkAsDownloading();
                        if (item.Type == DownloadType.Video)
                            await _youtubeService.DownloadVideoAsync(
                                item.VideoStreamInfo, item.AudioStreamInfo,
                                item.OutputPath, task.Progress, token
                            );
                        else
                            await _youtubeService.DownloadAudioAsync(
                                item.AudioStreamInfo,
                                item.OutputPath, task.Progress, token
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
