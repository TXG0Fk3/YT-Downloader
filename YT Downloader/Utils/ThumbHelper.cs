using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace YT_Downloader.Utils
{
    static class ThumbHelper
    {
        public static async Task<string> DownloadThumbnailAsync(string videoId)
        {
            string thumbnailUrl = $"https://img.youtube.com/vi/{videoId}/mqdefault.jpg";
            string tempFilePath = $"{Path.GetTempPath()}\\{videoId}.jpg";

            using (var httpClient = new HttpClient())
            {
                var content = await httpClient.GetByteArrayAsync(thumbnailUrl);
                await File.WriteAllBytesAsync(tempFilePath, content);
            }

            return tempFilePath;
        }
    }
}
