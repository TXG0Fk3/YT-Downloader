using YT_Downloader.Enums;

namespace YT_Downloader.Models.Info
{
    public class StreamOption
    {
        public string Quality { get; init; }
        public MediaFormat Format { get; init; }
        public double SizeMB { get; init; }
    }
}