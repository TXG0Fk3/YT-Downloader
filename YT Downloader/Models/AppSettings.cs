using YT_Downloader.Enums;

namespace YT_Downloader.Models
{
    public record AppSettings(
            ThemeOption Theme,
            string DefaultDownloadsPath,
            bool AlwaysAskWhereSave
        );
}
