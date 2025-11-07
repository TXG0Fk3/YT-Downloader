using System.Diagnostics;
using System.IO;
using System.Linq;

namespace YT_Downloader.Helpers
{
    public static class FileHelper
    {
        public static void OpenFolder(string FilePath) =>
            Process.Start("explorer.exe", Path.GetDirectoryName(FilePath) ?? string.Empty);

        public static void CreateFolder(string folderPath) =>
            Directory.CreateDirectory(folderPath);

        public static void DeleteFile(string filePath) =>
            File.Delete(filePath);

        public static void DeleteFolder(string folderPath) =>
            Directory.Delete(folderPath, true);

        public static string SanitizeFileName(string name) =>
            string.Concat(name.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
    }
}
