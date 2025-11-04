using System.Threading.Tasks;

namespace YT_Downloader.Messages
{
    public record FolderPickerRequestMessage(TaskCompletionSource<string?> Tcs);
}
