using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;
using YT_Downloader.Enums;
using YT_Downloader.Models.Info;

namespace YT_Downloader.Services
{
    public class YoutubeService
    {
        private readonly YoutubeClient _youtubeClient = new();

        public async Task<VideoInfo> GetVideoAsync(string videoUrl, CancellationToken token)
        {
            var video = await _youtubeClient.Videos.GetAsync(videoUrl, token);
            var streamManifest = await GetStreamManifestAsync(video.Id, token);

            List<StreamOption> streamOptions = new();

            streamOptions.AddRange(
                streamManifest
                    .GetVideoOnlyStreams()
                    .Where(s => s.Container == Container.Mp4)
                    .Select(s => new StreamOption
                    {
                        Quality = s.VideoQuality.Label,
                        Format = MediaFormat.Mp4,
                        SizeMB = s.Size.MegaBytes
                    })
            );

            var bestAudio = GetBestAudioOnlyStreamInfo(streamManifest);
            if (bestAudio != null)
            {
                streamOptions.Add(new StreamOption
                {
                    Quality = "Best",
                    Format = MediaFormat.Mp3,
                    SizeMB = bestAudio.Size.MegaBytes
                });
            }

            return new VideoInfo
            {
                Id = video.Id,
                Url = video.Url,
                Title = video.Title,
                Author = video.Author.ToString(),
                ThumbnailUrl = $"https://img.youtube.com/vi/{video.Id}/mqdefault.jpg",
                Streams = streamOptions,
                Manifest = streamManifest
            };
        }

        public async Task<PlaylistInfo> GetPlaylistAsync(string playlistUrl, CancellationToken token)
        {
            var playlist = await _youtubeClient.Playlists.GetAsync(playlistUrl, token);

            return new PlaylistInfo
            {
                Id = playlist.Id,
                Url = playlist.Url,
                Title = playlist.Title,
                Author = playlist.Author?.ToString() ?? "Unknown Author"
            };
        }

        public async Task<IReadOnlyList<VideoInfo>> GetPlaylistVideosAsync(string playlistId, CancellationToken token)
        {
            var semaphore = new SemaphoreSlim(16);

            return await Task.WhenAll(
                (await _youtubeClient.Playlists.GetVideosAsync(playlistId, token))
                .Select(async v =>
                {
                    await semaphore.WaitAsync(token);
                    try
                    {
                        return await GetVideoAsync(v.Url, token);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                })
            );
        }

        public async Task<IEnumerable<StreamManifest>> GetPlaylistStreamManifestsAsync(string playlistId, CancellationToken token)
        {
            var semaphore = new SemaphoreSlim(16);

            return await Task.WhenAll(
                (await GetPlaylistVideosAsync(playlistId, token))
                .Select(async v =>
                {
                    await semaphore.WaitAsync(token);
                    try
                    {
                        return await GetStreamManifestAsync(v.Id, token);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                })
            );
        }

        public VideoOnlyStreamInfo? GetVideoOnlyStreamInfo(StreamManifest streamManifest, string quality)
        {
            var targetResolution = ParseResolution(quality);
            var targetFps = ParseFps(quality);

            return streamManifest.GetVideoOnlyStreams()
                .Where(s => s.Container == Container.Mp4)
                .OrderBy(s => Score(s, targetResolution, targetFps))
                .FirstOrDefault();
        }

        public AudioOnlyStreamInfo? GetBestAudioOnlyStreamInfo(StreamManifest streamManifest) =>
            (AudioOnlyStreamInfo)streamManifest.GetAudioOnlyStreams().Where(s => s.Container == Container.Mp4).GetWithHighestBitrate();

        public async Task DownloadVideoAsync(
            StreamManifest streamManifest, StreamOption videoStreamOption,
            string outputPath, IProgress<double> progress,  
            CancellationToken token)
        {
            var videoStreamInfo = GetVideoOnlyStreamInfo(streamManifest, videoStreamOption.Quality);
            var audioStreamInfo = GetBestAudioOnlyStreamInfo(streamManifest);

            if (videoStreamInfo != null && audioStreamInfo != null)
                await _youtubeClient.Videos.DownloadAsync(new IStreamInfo[] { videoStreamInfo, audioStreamInfo },
                    new ConversionRequestBuilder(outputPath).Build(), progress, token);
        }

        public async Task DownloadAudioAsync(
            StreamManifest streamManifest,
            string outputPath, IProgress<double> progress,
            CancellationToken token)
        {
            var audioStreamInfo = GetBestAudioOnlyStreamInfo(streamManifest);

            if (audioStreamInfo != null)
                await _youtubeClient.Videos.DownloadAsync(new IStreamInfo[] { audioStreamInfo },
                    new ConversionRequestBuilder(outputPath).SetContainer("mp3").Build(), progress, token);
        }

        private async Task<StreamManifest> GetStreamManifestAsync(string videoId, CancellationToken token) =>
            await _youtubeClient.Videos.Streams.GetManifestAsync(videoId, token);

        private static int Score(VideoOnlyStreamInfo stream, int targetResolution, int targetFps)
        {
            var label = stream.VideoQuality.Label;
            var res = ParseResolution(label);
            var fps = ParseFps(label);
            return Math.Abs(res - targetResolution) + Math.Abs(fps - targetFps);
        }

        private static int ParseResolution(string text) => int.Parse(text.Split('p')[0]);
        private static int ParseFps(string text) => text.Split(' ')[0].EndsWith("60") ? 60 : 30;
    }
}
