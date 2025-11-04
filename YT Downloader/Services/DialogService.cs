using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Microsoft.Windows.Storage.Pickers;
using YT_Downloader.Views.Dialogs;

namespace YT_Downloader.Services
{
    public class DialogService
    {
        private XamlRoot _xamlRoot;

        public void Initialize(XamlRoot root) =>
            _xamlRoot = root;

        public async Task ShowDetailsDialogAsync()
        {
            var dialog = new DetailsDialog()
            {
                XamlRoot = _xamlRoot,
            };

            await dialog.ShowAsync();
        }

        public async Task<string?> OpenFolderPickerAsync()
        {
            var folderPicker = new FolderPicker(_xamlRoot.ContentIslandEnvironment.AppWindowId)
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };

            var folder = await folderPicker.PickSingleFolderAsync();
            return folder?.Path;
        }
    }
}
