using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace YT_Downloader.Utils
{
    static class ThumbHelper
    {
        public static async Task<string> DownloadThumbnailAsync(string videoId)
        {
            string tempDirectory = $"{Path.GetTempPath()}\\ThumbnailCache";
            string tempFilePath = $"{tempDirectory}\\{videoId}.jpg";

            // Garante que o diretório exista
            Directory.CreateDirectory(tempDirectory);

            using (var httpClient = new HttpClient())
            {
                var content = await httpClient.GetByteArrayAsync($"https://img.youtube.com/vi/{videoId}/mqdefault.jpg");
                await File.WriteAllBytesAsync(tempFilePath, content);
            }

            return tempFilePath;
        }
    }
}
