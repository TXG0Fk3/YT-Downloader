using System.Reflection;

namespace YT_Downloader.Helpers
{
    public static class AppInfoHelper
    {
        public static string Version { get; } =
            Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion
            ?? "Unknown";
    }
}
