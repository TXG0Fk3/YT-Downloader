namespace YT_Downloader.Models
{
    public record AppSettings(
            string Theme,
            string DefaultDownloadsPath,
            bool AlwaysAskWhereSave
        );
}
