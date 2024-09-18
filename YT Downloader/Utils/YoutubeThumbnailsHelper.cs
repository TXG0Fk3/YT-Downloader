using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using YoutubeExplode.Videos;

namespace YT_Downloader.Utils
{
    static class YoutubeThumbnailsHelper
    {
        public static async Task<string> DownloadThumbnailAsync(string thumbnailPath, Video video)
        {
            thumbnailPath += $"{video.Id}.jpg";
            string thumbnailUrl = $"https://img.youtube.com/vi/{video.Id}/mqdefault.jpg";

            using var httpClient = new HttpClient();
            var content = await httpClient.GetByteArrayAsync(thumbnailUrl);
            await File.WriteAllBytesAsync(thumbnailPath, content);

            return thumbnailPath;
        }
    }
}
