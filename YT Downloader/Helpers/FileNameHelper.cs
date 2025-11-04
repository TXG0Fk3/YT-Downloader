using System.IO;
using System.Linq;

namespace YT_Downloader.Helpers
{
    public static class FileNameHelper
    {
        public static string SanitizeFileName(string name) =>
            string.Concat(name.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
    }
}
