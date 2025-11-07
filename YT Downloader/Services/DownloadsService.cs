using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YT_Downloader.Enums;
using YT_Downloader.Helpers;
using YT_Downloader.Helpers.Builders;
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

        public async Task EnqueueDownloadable(IDownloadable downloadable)
        {
            if (downloadable is DownloadItem downloadItem)
                _downloadQueue.Enqueue(downloadItem);
            else if (downloadable is DownloadGroup downloadGroup)
                await EnqueuePlaylist(downloadGroup);
            else return;

            if (!_processing)
            {
                _processing = true;
                _ = ProcessQueueAsync();
            }
        }

        public async Task EnqueuePlaylist(DownloadGroup group)
        {
            try
            {
                var groupVideoInfos = await _youtubeService.GetPlaylistVideosAsync(group.PlaylistId, group.CTS.Token);
                group.CTS.Token.ThrowIfCancellationRequested();

                foreach (var videoInfo in groupVideoInfos)
                {
                    var builder = new DownloadItemBuilder().FromVideoInfo(videoInfo)
                        .WithOutputPath(Path.Combine(group.OutputPath, FileHelper.SanitizeFileName(videoInfo.Title)))
                        .WithGroupCancellation(group.CTS.Token);

                    var item = group.Type == DownloadType.Video
                        ? builder.AsVideo(group.Quality,
                            _youtubeService.GetClosestMp4StreamOption(videoInfo.Streams, group.Quality),
                            _youtubeService.GetBestMp3StreamOption(videoInfo.Streams)).Build()
                        : builder.AsAudio(_youtubeService.GetBestMp3StreamOption(videoInfo.Streams)).Build();

                    group.Items.Add(item);
                    _downloadQueue.Enqueue(item);
                }
            }
            catch (Exception ex)
            {
                group.Error = ex;
            }
        }

        private async Task ProcessQueueAsync()
        {
            while (true)
            {
                if (_downloadQueue.TryDequeue(out var item))
                {
                    _ = DownloadAsync(item);
                }
                else
                {
                    await Task.Delay(200);
                    if (_downloadQueue.IsEmpty)
                    {
                        _processing = false;
                        return;
                    }
                }
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
