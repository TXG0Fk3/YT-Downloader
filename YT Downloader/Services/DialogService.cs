using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using YT_Downloader.Models;
using YT_Downloader.ViewModels.Dialogs;
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

            return null;
        }
    }
}
