using System.Collections.Generic;

namespace YT_Downloader.Utils
{
    class YoutubePlaylistHelper
    {
        public HashSet<string> GetResolutions() => [
            "4320p", "2160p", "1440p", "1080p", "720p", "480p", "360p", "240p", "144p"
            ];
    }
}
