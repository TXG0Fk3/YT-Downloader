using Microsoft.UI.Xaml.Controls;
using YT_Downloader.ViewModels.Dialogs;

namespace YT_Downloader.Views.Dialogs
{
    public sealed partial class DetailsDialog : ContentDialog
    {
        public DetailsDialog(DetailsDialogViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
