using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Converter;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YT_Downloader.Services
{
    public class YoutubeService
    {
        private readonly YoutubeClient _youtubeClient = new();

        public async Task<IVideo> GetVideoAsync(string videoUrl, CancellationToken token) =>
            await _youtubeClient.Videos.GetAsync(videoUrl, token);

        public async Task<IPlaylist> GetPlaylistAsync(string playlistUrl, CancellationToken token) =>
            await _youtubeClient.Playlists.GetAsync(playlistUrl, token);

        public async Task<IReadOnlyList<IVideo>> GetPlaylistVideosAsync(string playlistId, CancellationToken token) =>
            await _youtubeClient.Playlists.GetVideosAsync(playlistId, token);

        public async Task<StreamManifest> GetStreamManifestAsync(string videoId, CancellationToken token) =>
            await _youtubeClient.Videos.Streams.GetManifestAsync(videoId, token);

        public async Task<IReadOnlyList<StreamManifest>> GetPlaylistStreamManifestsAsync(Playlist playlist, CancellationToken token)
        {
            var semaphore = new SemaphoreSlim(16);

            return await Task.WhenAll(
                (await GetPlaylistVideosAsync(playlist.Id, token))
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

        public async Task DownloadVideoAsync(
            VideoOnlyStreamInfo videoStreamInfo, AudioOnlyStreamInfo audioStreamInfo,
            string outputPath, IProgress<double> progress,  
            CancellationToken token)
        {
            await _youtubeClient.Videos.DownloadAsync(new IStreamInfo[] { videoStreamInfo, audioStreamInfo },
                new ConversionRequestBuilder(outputPath).Build(), progress, token);
        }

        public async Task DownloadAudioAsync(
            AudioOnlyStreamInfo audioStreamInfo,
            string outputPath, IProgress<double> progress,
            CancellationToken token)
        {
            await _youtubeClient.Videos.DownloadAsync(new IStreamInfo[] { audioStreamInfo },
                new ConversionRequestBuilder(outputPath).Build(), progress, token);
        }
    }
}
